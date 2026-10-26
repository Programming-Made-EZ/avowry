using Phrase.Compiler.Syntax;

namespace Phrase.Compiler.Semantics;

/// <summary>
/// Binds the syntax tree into the <see cref="ModuleModel"/>: every phrase call matched to
/// exactly one template, every value typed, every spec checked.
/// </summary>
public static class Resolver
{
    /// <summary>Parses then resolves. Parse diagnostics come first in the result.</summary>
    public static ResolveResult Resolve(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        var parsed = Parser.Parse(source);
        var resolved = Resolve(parsed.File);
        return new ResolveResult(resolved.Module, [.. parsed.Diagnostics, .. resolved.Diagnostics]);
    }

    /// <summary>Resolves an already-parsed file. Must never throw, even on a tree with syntax errors.</summary>
    public static ResolveResult Resolve(SourceFile file)
    {
        ArgumentNullException.ThrowIfNull(file);
        return new ResolutionPass(file).Run();
    }

    private sealed class ResolutionPass(SourceFile file)
    {
        private readonly List<Diagnostic> diagnostics = [];
        private readonly HashSet<string> typeNames = new(PType.Core.Keys, StringComparer.Ordinal);
        private readonly Dictionary<string, IReadOnlyList<FieldModel>> recordFields = new(StringComparer.Ordinal);
        private readonly List<(PhraseDecl Declaration, PhraseModel Model)> phraseDeclarations = [];

        public ResolveResult Run()
        {
            RegisterTypeNames();
            var types = file.Types.Select(ResolveTypeDeclaration).ToList();
            foreach (var type in types)
                recordFields[type.Name] = type.Fields;
            var phrases = ResolvePhrases();
            var endpoints = ResolveEndpoints();
            var module = new ModuleModel(file.Module?.Name ?? string.Empty, types, phrases, endpoints);
            return new ResolveResult(module, diagnostics);
        }

        private void RegisterTypeNames()
        {
            foreach (var declaration in file.Types)
            {
                if (!typeNames.Add(declaration.Name))
                    Report("PH302", $"Type '{declaration.Name}' is already defined.", declaration.At);
            }
        }

        private TypeModel ResolveTypeDeclaration(TypeDecl declaration) =>
            new(declaration.Name, ResolveFields(declaration.Fields, declaration.Name));

        private IReadOnlyList<PhraseModel> ResolvePhrases()
        {
            var phrases = new List<PhraseModel>();
            var skeletons = new Dictionary<string, PhraseModel>(StringComparer.OrdinalIgnoreCase);
            foreach (var declaration in file.Phrases)
            {
                var parts = new List<PhrasePart>();
                var holes = new List<FieldModel>();
                var holeNames = new HashSet<string>(StringComparer.Ordinal);
                foreach (var part in declaration.Template.Parts)
                {
                    switch (part)
                    {
                        case TemplateWord word:
                            parts.Add(new PhrasePart(word.Text, false));
                            break;
                        case TemplateHole hole:
                            parts.Add(new PhrasePart(hole.Name, true));
                            // The parser already reports PH206. Keep the first so a later scope
                            // or call lookup has one entry per name and does not throw.
                            if (holeNames.Add(hole.Name))
                                holes.Add(new FieldModel(hole.Name, ResolveType(hole.Type)));
                            break;
                        default:
                            throw new InvalidOperationException($"Unknown template part {part.GetType().Name}.");
                    }
                }
                var model = new PhraseModel(
                    Key(declaration.Template), SourceFormatter.Format(declaration.Template), parts, holes,
                    declaration.Returns is null ? null : ResolveType(declaration.Returns),
                    declaration.Effects, declaration.Failures,
                    declaration.Natives.ToDictionary(n => n.Target, n => n.Code, StringComparer.Ordinal),
                    declaration.Steps is null ? null : [], []);
                var skeleton = Skeleton(declaration.Template);
                if (skeletons.TryGetValue(skeleton, out var existing))
                {
                    Report("PH010", $"Phrase '{model.Template}' has the same wording as '{existing.Template}'.", declaration.At);
                    continue;
                }
                skeletons.Add(skeleton, model);
                phrases.Add(model);
                phraseDeclarations.Add((declaration, model));
            }

            for (var i = 0; i < phraseDeclarations.Count; i++)
            {
                var (declaration, model) = phraseDeclarations[i];
                if (declaration.Steps is null)
                    continue;
                if (model.Returns is not null)
                    Report("PH325", $"Composite phrase '{model.Template}' cannot return a value.", declaration.At);
                var scope = model.Holes.ToDictionary(hole => hole.Name, hole => hole.Type, StringComparer.Ordinal);
                var steps = ResolveSteps(declaration.Steps, scope, null, model.Failures);
                var resolved = model with { Steps = steps };
                phraseDeclarations[i] = (declaration, resolved);
                phrases[i] = resolved;
            }

            ResolvePhraseFlow(phrases);
            var declarationsByKey = phraseDeclarations.ToDictionary(pair => pair.Model.Key, pair => pair.Declaration, StringComparer.Ordinal);
            for (var i = 0; i < phrases.Count; i++)
            {
                var declaration = declarationsByKey[phrases[i].Key];
                var specs = ResolvePhraseSpecs(declaration, phrases[i]);
                phrases[i] = phrases[i] with { Specs = specs };
            }
            return phrases;
        }

        private IReadOnlyList<EndpointModel> ResolveEndpoints()
        {
            var endpoints = new List<EndpointModel>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            var routes = new HashSet<(string Verb, string Route)>();
            foreach (var declaration in file.Endpoints)
            {
                if (!names.Add(declaration.Name))
                    Report("PH304", $"Endpoint '{declaration.Name}' is already defined.", declaration.At);
                if (!routes.Add((declaration.Verb, declaration.Route)))
                    Report("PH304", $"Endpoint route '{declaration.Verb} {declaration.Route}' is already defined.", declaration.At);
                var input = ResolveFields(declaration.Input, $"endpoint '{declaration.Name}' input");
                var output = ResolveFields(declaration.Output, $"endpoint '{declaration.Name}' output");
                var inputType = PType.EndpointInput(declaration.Name);
                var outputType = declaration.Output.Count == 0 ? null : PType.EndpointOutput(declaration.Name);
                recordFields[inputType.Name] = input;
                if (outputType is not null)
                    recordFields[outputType.Name] = output;
                var scope = new Dictionary<string, PType>(StringComparer.Ordinal) { ["input"] = inputType };
                var steps = ResolveSteps(declaration.Steps, scope, outputType, null);
                if (outputType is not null && !steps.Any(step => step is BoundRespondStep or BoundFailStep))
                    Report("PH323", $"Endpoint '{declaration.Name}' has an output but never responds or fails.", declaration.At);
                var failures = EffectiveFailures(steps);
                var model = new EndpointModel(
                    declaration.Name, declaration.Verb, declaration.Route,
                    inputType, input, outputType, output, steps, failures, []);
                endpoints.Add(model with { Specs = ResolveEndpointSpecs(declaration, model) });
            }
            return endpoints;
        }

        private IReadOnlyList<BoundStep> ResolveSteps(
            IReadOnlyList<Step> steps, Dictionary<string, PType> scope, PType? outputType,
            IReadOnlyList<string>? declaredFailures)
        {
            var bound = new List<BoundStep>();
            var terminated = false;
            foreach (var step in steps)
            {
                if (terminated)
                    Report("PH324", "Step is unreachable after 'respond with' or 'fail with'.", step.At);
                switch (step)
                {
                    case PhraseCall call:
                    {
                        var resolved = ResolveCall(call, scope);
                        if (resolved is not null)
                        {
                            if (resolved.Value.Phrase.Returns is not null)
                                Report("PH321", $"Phrase '{resolved.Value.Phrase.Template}' returns {resolved.Value.Phrase.Returns}; use 'let' to keep it.", call.At);
                            bound.Add(new BoundCallStep(resolved.Value.Call, SourceFormatter.Format(call)));
                        }
                        break;
                    }
                    case LetStep let:
                    {
                        var resolved = ResolveCall(let.Call, scope);
                        var type = PType.Error;
                        if (resolved is not null)
                        {
                            type = resolved.Value.Phrase.Returns ?? PType.Error;
                            if (resolved.Value.Phrase.Returns is null)
                                Report("PH320", $"Phrase '{resolved.Value.Phrase.Template}' returns no value and cannot be used with 'let'.", let.At);
                        }
                        if (scope.ContainsKey(let.Name))
                            Report("PH313", $"Variable '{let.Name}' is already defined.", let.At);
                        else
                            scope.Add(let.Name, type);
                        if (resolved is not null)
                            bound.Add(new BoundLetStep(let.Name, type, resolved.Value.Call, SourceFormatter.Format(let)));
                        break;
                    }
                    case FailStep fail:
                        if (declaredFailures is not null && !declaredFailures.Contains(fail.Code, StringComparer.Ordinal))
                            Report("PH326", $"Composite failure '{fail.Code}' is not declared in 'fails with'.", fail.At);
                        bound.Add(new BoundFailStep(fail.Code, SourceFormatter.Format(fail)));
                        terminated = true;
                        break;
                    case RespondStep respond:
                        if (outputType is null)
                        {
                            Report("PH322", "'respond with' is only valid in an endpoint with an output.", respond.At);
                            bound.Add(new BoundRespondStep(new BoundRecord([], PType.Error), SourceFormatter.Format(respond)));
                        }
                        else
                        {
                            bound.Add(new BoundRespondStep(ResolveRecord(respond.Value, outputType, scope), SourceFormatter.Format(respond)));
                        }
                        terminated = true;
                        break;
                }
            }
            return bound;
        }

        private void ResolvePhraseFlow(List<PhraseModel> phrases)
        {
            var byKey = phrases.ToDictionary(phrase => phrase.Key, StringComparer.Ordinal);
            var visiting = new HashSet<string>(StringComparer.Ordinal);
            var complete = new Dictionary<string, (IReadOnlyList<string> Failures, IReadOnlySet<string> Effects)>(StringComparer.Ordinal);
            var reportedCycles = new HashSet<string>(StringComparer.Ordinal);

            (IReadOnlyList<string> Failures, IReadOnlySet<string> Effects) Visit(PhraseModel phrase)
            {
                if (complete.TryGetValue(phrase.Key, out var cached))
                    return cached;
                if (!visiting.Add(phrase.Key))
                {
                    if (reportedCycles.Add(phrase.Key))
                        Report("PH327", $"Phrase '{phrase.Template}' participates in a call cycle.", PhraseDeclaration(phrase.Key).At);
                    return ([], new HashSet<string>(StringComparer.Ordinal));
                }

                var failures = phrase.Failures.ToHashSet(StringComparer.Ordinal);
                var effects = phrase.Effects.ToHashSet(StringComparer.Ordinal);
                foreach (var step in phrase.Steps ?? [])
                {
                    if (step is BoundFailStep fail)
                        failures.Add(fail.Code);
                    if (step is not BoundCallStep and not BoundLetStep)
                        continue;
                    var key = CalledPhraseKey(step)!;
                    var callee = byKey[key];
                    var inherited = Visit(callee);
                    failures.UnionWith(inherited.Failures);
                    effects.UnionWith(inherited.Effects);
                    foreach (var effect in inherited.Effects.Except(phrase.Effects, StringComparer.Ordinal))
                        Report("PH328", $"Composite phrase '{phrase.Template}' must declare effect '{effect}'.", PhraseDeclaration(phrase.Key).At);
                }
                visiting.Remove(phrase.Key);
                return complete[phrase.Key] = (failures.Order(StringComparer.Ordinal).ToList(), effects);
            }

            for (var i = 0; i < phrases.Count; i++)
            {
                var flow = Visit(phrases[i]);
                phrases[i] = phrases[i] with { Failures = flow.Failures };
                var declarationIndex = phraseDeclarations.FindIndex(pair => pair.Model.Key == phrases[i].Key);
                phraseDeclarations[declarationIndex] = (phraseDeclarations[declarationIndex].Declaration, phrases[i]);
            }
        }

        private IReadOnlyList<string> EffectiveFailures(IReadOnlyList<BoundStep> steps)
        {
            var failures = new HashSet<string>(StringComparer.Ordinal);
            foreach (var step in steps)
            {
                if (step is BoundFailStep fail)
                    failures.Add(fail.Code);
                var key = CalledPhraseKey(step);
                if (key is not null)
                    failures.UnionWith(phraseDeclarations.Single(pair => pair.Model.Key == key).Model.Failures);
            }
            return failures.Order(StringComparer.Ordinal).ToList();
        }

        private IReadOnlyList<PhraseSpecModel> ResolvePhraseSpecs(PhraseDecl declaration, PhraseModel phrase) =>
            declaration.Specs.Select(spec => ResolvePhraseSpec(spec, phrase)).ToList();

        private PhraseSpecModel ResolvePhraseSpec(SpecDecl spec, PhraseModel phrase)
        {
            if (spec.When is not SpecTrigger.Called)
            {
                Report("PH330", $"Spec '{spec.Description}' on phrase '{phrase.Template}' must use 'when called'.", spec.At);
            }

            var scope = phrase.Holes.ToDictionary(h => h.Name, h => h.Type, StringComparer.Ordinal);
            var givens = new List<BoundArgument>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var hasInvalidBinding = false;
            foreach (var given in spec.Givens)
            {
                if (given is GivenInput)
                {
                    Report("PH331", "Phrase specs cannot bind input; use a hole binding.", given.At);
                    continue;
                }

                var binding = (GivenBinding)given;
                if (!scope.TryGetValue(binding.Name, out var type))
                {
                    hasInvalidBinding = true;
                    Report("PH331", $"Phrase spec for '{phrase.Template}' does not bind hole '{binding.Name}'. Valid holes: {string.Join(", ", scope.Keys.Order(StringComparer.Ordinal))}.", binding.At);
                    continue;
                }
                if (!seen.Add(binding.Name))
                {
                    hasInvalidBinding = true;
                    Report("PH331", $"Hole '{binding.Name}' is bound more than once in the spec.", binding.At);
                    continue;
                }
            }

            foreach (var hole in phrase.Holes)
            {
                var bindings = spec.Givens.OfType<GivenBinding>().Where(g => g.Name == hole.Name).ToList();
                if (bindings.Count == 0)
                {
                    if (!hasInvalidBinding)
                        Report("PH331", $"Spec for '{phrase.Template}' is missing the hole '{hole.Name}'.", spec.At);
                    continue;
                }

                var binding = bindings[0];
                if (ContainsPath(binding.Value))
                {
                    Report("PH332", $"Spec value for '{binding.Name}' must be a literal, not a path.", binding.At);
                    continue;
                }
                givens.Add(new BoundArgument(new FieldModel(hole.Name, hole.Type), ResolveValue(binding.Value, hole.Type, scope)));
            }

            var outcome = ResolveSpecOutcome(
                spec,
                phrase,
                scope,
                outputType: null,
                endpointFailures: null,
                endpointName: null);
            return new PhraseSpecModel(spec.Description, givens, outcome, SourceFormatter.Lines(spec));
        }

        private IReadOnlyList<EndpointSpecModel> ResolveEndpointSpecs(EndpointDecl declaration, EndpointModel endpoint) =>
            declaration.Specs.Select(spec => ResolveEndpointSpec(spec, endpoint)).ToList();

        private EndpointSpecModel ResolveEndpointSpec(SpecDecl spec, EndpointModel endpoint)
        {
            if (spec.When is not SpecTrigger.Handled)
            {
                Report("PH330", $"Spec '{spec.Description}' on endpoint '{endpoint.Name}' must use 'when handled'.", spec.At);
            }

            var scope = new Dictionary<string, PType>(StringComparer.Ordinal) { ["input"] = endpoint.InputType };
            var seenInput = false;
            var inputFields = new List<BoundRecordField>();
            foreach (var given in spec.Givens)
            {
                if (given is GivenInput inputGiven)
                {
                    seenInput = true;
                    if (ContainsPath(inputGiven.Value))
                    {
                        Report("PH332", "Spec input must contain literal values, not paths.", inputGiven.At);
                        continue;
                    }
                    var inputRecord = ResolveRecord(inputGiven.Value, endpoint.InputType, scope);
                    inputFields = [.. inputRecord.Fields];
                    continue;
                }

                var binding = (GivenBinding)given;
                if (binding.Name == "input")
                {
                    Report("PH331", $"Endpoint spec for '{endpoint.Name}' must use 'given input {{ ... }}', not 'input = ...'.", binding.At);
                    continue;
                }
                Report("PH331", $"Endpoint spec for '{endpoint.Name}' must use 'given input {{ ... }}', not '{binding.Name}'.", binding.At);
            }
            if (!seenInput && spec.Givens.Count == 0)
                Report("PH331", $"Endpoint spec for '{endpoint.Name}' is missing the required 'given input {{ ... }}'.", spec.At);
            if (spec.Givens.Count(g => g is GivenInput) > 1)
                Report("PH331", $"Endpoint spec for '{endpoint.Name}' has more than one input binding.", spec.At);

            var outcome = ResolveSpecOutcome(spec, null, scope, endpoint.OutputType, endpoint.Failures, endpoint.Name);
            var boundInput = new BoundRecord(inputFields, endpoint.InputType);
            return new EndpointSpecModel(spec.Description, boundInput, outcome, SourceFormatter.Lines(spec));
        }

        private SpecOutcomeModel ResolveSpecOutcome(
            SpecDecl spec,
            PhraseModel? phrase,
            IReadOnlyDictionary<string, PType> scope,
            PType? outputType,
            IReadOnlyList<string>? endpointFailures,
            string? endpointName)
        {
            var outcomes = spec.Thens;
            if (outcomes.Count > 1)
            {
                Report("PH335", $"Spec '{spec.Description}' has more than one outcome.", spec.At);
                return new SucceedsModel();
            }

            if (outcomes.Count == 0)
                return new SucceedsModel();

            var outcome = outcomes[0];
            switch (outcome)
            {
                case ReturnsOutcome returns:
                    if (phrase is null || phrase.Returns is null)
                    {
                        Report("PH333", $"Spec '{spec.Description}' uses 'returns' but this unit cannot return a value.", returns.At);
                        return new SucceedsModel();
                    }
                    if (ContainsPath(returns.Value))
                    {
                        Report("PH332", "Spec outcome value must be a literal, not a path.", returns.At);
                        return new SucceedsModel();
                    }
                    var value = ResolveValue(returns.Value, phrase.Returns, scope);
                    return new ReturnsModel(value);
                case RespondsOutcome responds:
                    if (outputType is null)
                    {
                        Report("PH333", $"Spec '{spec.Description}' uses 'responds' but this unit has no output.", responds.At);
                        return new SucceedsModel();
                    }
                    if (ContainsPath(responds.Value))
                    {
                        Report("PH332", "Spec outcome value must contain literal values, not paths.", responds.At);
                        return new RespondsModel(new BoundRecord([], outputType));
                    }
                    var responseRecord = ResolveRecord(responds.Value, outputType, scope);
                    return new RespondsModel(responseRecord);
                case FailsWithOutcome fail:
                    if (phrase is not null && !phrase.Failures.Contains(fail.Code, StringComparer.Ordinal))
                    {
                        Report("PH334", $"Phrase '{phrase.Template}' cannot fail with '{fail.Code}'. Valid options: {string.Join(", ", phrase.Failures)}.", fail.At);
                    }
                    if (phrase is null && endpointFailures is not null && !endpointFailures.Contains(fail.Code, StringComparer.Ordinal))
                    {
                        Report("PH334", $"Endpoint '{endpointName}' cannot fail with '{fail.Code}'. Valid options: {string.Join(", ", endpointFailures)}.", fail.At);
                    }
                    return new FailsModel(fail.Code);
                case SucceedsOutcome:
                    if (phrase is not null && phrase.Returns is not null)
                    {
                        Report("PH333", $"Spec '{spec.Description}' expects success for a phrase that returns a value.", outcome.At);
                    }
                    return new SucceedsModel();
                default:
                    throw new InvalidOperationException();
            }
        }

        private static bool ContainsPath(Expr expression) => expression switch
        {
            PathExpr => true,
            RecordExpr record => record.Fields.Any(field => ContainsPath(field.Value)),
            ListExpr list => list.Items.Any(ContainsPath),
            _ => false,
        };

        private static string? CalledPhraseKey(BoundStep step) => step switch
        {
            BoundCallStep call => call.Call.PhraseKey,
            BoundLetStep let => let.Call.PhraseKey,
            _ => null,
        };

        private PhraseDecl PhraseDeclaration(string key) =>
            phraseDeclarations.Single(pair => pair.Model.Key == key).Declaration;

        private (BoundCall Call, PhraseModel Phrase)? ResolveCall(
            PhraseCall call, IReadOnlyDictionary<string, PType> scope)
        {
            var matches = phraseDeclarations.Where(p => Matches(p.Declaration.Template, call, scope)).ToList();
            if (matches.Count == 0)
            {
                Report("PH001", NoMatchMessage(call, scope), call.At);
                return null;
            }
            if (matches.Count > 1)
            {
                Report("PH002", $"Step is ambiguous between: {string.Join(", ", matches.Select(m => $"'{m.Model.Template}'"))}.", call.At);
                return null;
            }

            var match = matches[0];
            var arguments = new List<BoundArgument>();
            for (var i = 0; i < call.Parts.Count; i++)
            {
                if (match.Declaration.Template.Parts[i] is not TemplateHole hole)
                    continue;
                var field = match.Model.Holes.Single(h => h.Name == hole.Name);
                var value = call.Parts[i] switch
                {
                    CallArgument argument => ResolveValue(argument.Value, field.Type, scope),
                    CallWord word when word.Text is "true" or "false" =>
                        ResolveValue(new BoolLit(word.Text == "true", word.At), field.Type, scope),
                    CallWord word when word.Text == "none" =>
                        ResolveValue(new NoneLit(word.At), field.Type, scope),
                    CallWord word => ResolveValue(new PathExpr([word.Text], word.At), field.Type, scope),
                    _ => throw new InvalidOperationException()
                };
                arguments.Add(new BoundArgument(field, value));
            }
            return (new BoundCall(match.Model.Key, arguments), match.Model);
        }

        private static bool Matches(Template template, PhraseCall call, IReadOnlyDictionary<string, PType> scope) =>
            template.Parts.Count == call.Parts.Count && template.Parts.Zip(call.Parts).All(pair => pair.First switch
            {
                TemplateWord word => pair.Second is CallWord callWord &&
                    string.Equals(word.Text, callWord.Text, StringComparison.OrdinalIgnoreCase),
                TemplateHole => pair.Second is CallArgument || pair.Second is CallWord variable &&
                    (scope.ContainsKey(variable.Text) || IsLiteral(variable.Text)),
                _ => false,
            });

        private static bool IsLiteral(string word) => word is "true" or "false" or "none";

        private BoundExpr ResolveValue(Expr expression, PType expected, IReadOnlyDictionary<string, PType> scope)
        {
            var target = expected.IsOption ? expected.Arguments[0] : expected;
            var value = expression switch
            {
                PathExpr path => ResolvePath(path, scope),
                StringLit text => new BoundText(text.Value, target is { Name: "Text" or "Id" } ? target : PType.Text),
                NumberLit number => new BoundNumber(number.Text, number.Text.Contains('.', StringComparison.Ordinal) ? PType.Decimal : target is { Name: "Int" or "Decimal" } ? target : PType.Int),
                BoolLit boolean => new BoundBool(boolean.Value),
                NoneLit => new BoundNone(expected.IsOption ? expected : PType.OptionOf(PType.Error)),
                RecordExpr record => ResolveRecord(record, target, scope),
                ListExpr list => ResolveList(list, target, scope),
                _ => throw new InvalidOperationException($"Unknown expression {expression.GetType().Name}.")
            };
            if (!Compatible(value.Type, expected))
                Report("PH312", $"Expected {expected}, but got {value.Type}.", expression.At);
            return value;
        }

        private BoundExpr ResolvePath(PathExpr path, IReadOnlyDictionary<string, PType> scope)
        {
            if (path.Segments.Count == 0 || !scope.TryGetValue(path.Segments[0], out var type))
            {
                Report("PH310", $"Unknown variable '{path.Segments.FirstOrDefault()}'. In scope: {string.Join(", ", scope.Keys)}.", path.At);
                return new BoundVariable(path.Segments.FirstOrDefault() ?? "?", PType.Error);
            }
            BoundExpr value = new BoundVariable(path.Segments[0], type);
            foreach (var segment in path.Segments.Skip(1))
            {
                if (value.Type.IsError)
                    return new BoundField(value, segment, PType.Error);
                if (!recordFields.TryGetValue(value.Type.Name, out var fields))
                {
                    Report("PH311", $"Type {value.Type} has no fields; cannot read '{segment}'.", path.At);
                    return new BoundField(value, segment, PType.Error);
                }
                var field = fields.FirstOrDefault(f => f.Name == segment);
                if (field is null)
                {
                    Report("PH311", $"Unknown field '{segment}' on {value.Type}. Available fields: {string.Join(", ", fields.Select(f => f.Name))}.", path.At);
                    return new BoundField(value, segment, PType.Error);
                }
                value = new BoundField(value, field.Name, field.Type);
            }
            return value;
        }

        private BoundRecord ResolveRecord(RecordExpr record, PType expected, IReadOnlyDictionary<string, PType> scope)
        {
            if (!recordFields.TryGetValue(expected.Name, out var expectedFields))
                return new BoundRecord([], new PType("Record", []));
            var actualNames = record.Fields.Select(f => f.Name).ToHashSet(StringComparer.Ordinal);
            if (!actualNames.SetEquals(expectedFields.Select(f => f.Name)))
                Report("PH314", $"Record fields do not match {expected}.", record.At);
            var fields = expectedFields.Select(field =>
            {
                var actual = record.Fields.FirstOrDefault(f => f.Name == field.Name);
                return new BoundRecordField(field.Name, actual is null ? new BoundVariable("?", PType.Error) : ResolveValue(actual.Value, field.Type, scope));
            }).ToList();
            return new BoundRecord(fields, expected);
        }

        private BoundList ResolveList(ListExpr list, PType expected, IReadOnlyDictionary<string, PType> scope)
        {
            if (!expected.IsList)
                return new BoundList([], PType.ListOf(PType.Error));
            return new BoundList(list.Items.Select(item => ResolveValue(item, expected.Arguments[0], scope)).ToList(), expected);
        }

        private static bool Compatible(PType actual, PType expected) =>
            actual.IsError || expected.IsError || actual == expected ||
            expected.IsOption && (expected.Arguments[0].IsError || actual == expected.Arguments[0]);

        private string? BareFieldHint(PhraseCall call, IReadOnlyDictionary<string, PType> scope)
        {
            foreach (var word in call.Parts.OfType<CallWord>())
                foreach (var variable in scope)
                    if (recordFields.TryGetValue(variable.Value.Name, out var fields) && fields.Any(f => f.Name == word.Text))
                        return $"{variable.Key}.{word.Text}";
            return null;
        }

        private string NoMatchMessage(PhraseCall call, IReadOnlyDictionary<string, PType> scope)
        {
            const string noMatch = "No phrase matches this step.";
            var closest = phraseDeclarations
                .OrderBy(phrase => Distance(CallShape(call), TemplateShape(phrase.Declaration.Template)))
                .Select(phrase => phrase.Model)
                .FirstOrDefault();
            var suggestion = closest is null ? string.Empty : $" Closest: \"{closest.Template}\".";
            var hint = BareFieldHint(call, scope);
            return hint is null ? noMatch + suggestion : $"{noMatch}{suggestion} Did you mean '{hint}'?";
        }

        private static string CallShape(PhraseCall call) => string.Join(" ", call.Parts.Select(p => p is CallWord w ? w.Text : "_"));
        private static string TemplateShape(Template template) => string.Join(" ", template.Parts.Select(p => p is TemplateWord w ? w.Text : "_"));

        private static int Distance(string left, string right)
        {
            var a = left.Split(' ');
            var b = right.Split(' ');
            var costs = Enumerable.Range(0, b.Length + 1).ToArray();
            for (var i = 1; i <= a.Length; i++)
            {
                var previous = costs[0];
                costs[0] = i;
                for (var j = 1; j <= b.Length; j++)
                {
                    var old = costs[j];
                    costs[j] = Math.Min(Math.Min(costs[j] + 1, costs[j - 1] + 1), previous + (a[i - 1].Equals(b[j - 1], StringComparison.OrdinalIgnoreCase) ? 0 : 1));
                    previous = old;
                }
            }
            return costs[^1];
        }

        private IReadOnlyList<FieldModel> ResolveFields(IReadOnlyList<FieldDecl> declarations, string owner)
        {
            var fields = new List<FieldModel>();
            var names = new HashSet<string>(StringComparer.Ordinal);
            foreach (var field in declarations)
            {
                if (!names.Add(field.Name))
                {
                    Report("PH303", $"Field '{field.Name}' is repeated in {owner}.", field.At);
                    continue;
                }
                fields.Add(new FieldModel(field.Name, ResolveType(field.Type)));
            }
            return fields;
        }

        private PType ResolveType(TypeRef reference)
        {
            if (!typeNames.Contains(reference.Name))
            {
                Report("PH300", $"Unknown type '{reference.Name}'.", reference.At);
                return PType.Error;
            }
            var expectedArity = PType.Core.TryGetValue(reference.Name, out var arity) ? arity : 0;
            if (reference.Arguments.Count != expectedArity)
            {
                Report("PH301", $"Type '{reference.Name}' expects {expectedArity} type argument(s), but got {reference.Arguments.Count}.", reference.At);
                return PType.Error;
            }
            return new PType(reference.Name, reference.Arguments.Select(ResolveType).ToList());
        }

        private static string Key(Template template) => string.Join(" ", template.Parts.Select(part => part switch
        {
            TemplateWord word => word.Text.ToLowerInvariant(),
            TemplateHole => "_",
            _ => string.Empty,
        }));

        private static string Skeleton(Template template) =>
            string.Join(" ", template.Parts.OfType<TemplateWord>().Select(word => word.Text));

        private void Report(string code, string message, Position at) =>
            diagnostics.Add(new Diagnostic(code, message, at.Line, at.Column));
    }
}
