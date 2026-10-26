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
        return new DeclarationResolver(file).Run();
    }

    private sealed class DeclarationResolver(SourceFile file)
    {
        private readonly List<Diagnostic> diagnostics = [];
        private readonly HashSet<string> typeNames = new(PType.Core.Keys, StringComparer.Ordinal);

        public ResolveResult Run()
        {
            RegisterTypeNames();
            var types = file.Types.Select(ResolveTypeDeclaration).ToList();
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
                var parts = declaration.Template.Parts.Select(part => part switch
                {
                    TemplateWord word => new PhrasePart(word.Text, false),
                    TemplateHole hole => new PhrasePart(hole.Name, true),
                    _ => throw new InvalidOperationException($"Unknown template part {part.GetType().Name}.")
                }).ToList();
                var holes = declaration.Template.Parts.OfType<TemplateHole>()
                    .Select(hole => new FieldModel(hole.Name, ResolveType(hole.Type))).ToList();
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
                endpoints.Add(new EndpointModel(
                    declaration.Name, declaration.Verb, declaration.Route,
                    PType.EndpointInput(declaration.Name), input,
                    declaration.Output.Count == 0 ? null : PType.EndpointOutput(declaration.Name), output,
                    [], [], []));
            }
            return endpoints;
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
