using System.Text;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

namespace Phrase.Adapters.CSharp;

/// <summary>
/// Emits a C# API project and an xUnit specs project.
/// The hand-written shape to match is reference/csharp. Leaf phrases keep
/// their native blocks. A composite phrase calls its leaves in source order
/// and binds each result to a local, the same shape as reference/csharp.
/// </summary>
public sealed class CSharpProvider : ITargetAdapter
{
    public string Target => "csharp";

    public CompilationOutput Emit(ModuleModel module)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (module.Name.Length == 0)
            throw new NotImplementedException("The C# emitter needs a module name.");

        foreach (var phrase in module.Phrases)
            RequireSupportedPhrase(module, phrase);

        var api = module.Name + ".Api";
        var specs = module.Name + ".Specs";
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [$"{api}/{module.Name}.Api.csproj"] = ApiProject(module.Name),
            [$"{api}/Program.cs"] = Program(module),
            [$"{api}/PhraseRuntime.cs"] = Runtime(module.Name),
            [$"{api}/Types.cs"] = Types(module),
            [$"{api}/Phrases.cs"] = Phrases(module),
            [$"{specs}/{module.Name}.Specs.csproj"] = SpecsProject(module.Name),
            [$"{specs}/PhraseSpecs.cs"] = PhraseSpecs(module),
        };
        var endpointFiles = new Dictionary<string, string>(StringComparer.Ordinal);

        if (module.BuildId.Length > 0)
            files[$"{api}/PhraseBuild.cs"] = "using System.Reflection;\n\n[assembly: AssemblyMetadata(\"PhraseBuildId\", \"" + module.BuildId + "\")]\n";

        foreach (var endpoint in module.Endpoints)
        {
            var endpointPath = $"{api}/{EndpointFileName(endpoint.Name)}.cs";
            endpointFiles[endpoint.Name] = endpointPath;
            files[endpointPath] = EndpointFile(module, endpoint);
            files[$"{specs}/{EndpointSpecsFileName(endpoint.Name)}.cs"] = EndpointSpecs(module, endpoint);
        }

        return new CompilationOutput(files)
        {
            Module = module,
            EndpointFiles = endpointFiles,
            PhraseFiles = module.Phrases.ToDictionary(phrase => phrase.Key, _ => $"{api}/Phrases.cs", StringComparer.Ordinal),
        };
    }

    private static void RequireSupportedPhrase(ModuleModel module, PhraseModel phrase)
    {
        if (phrase.Effects.Count > 1)
            throw new NotImplementedException($"The C# emitter does not support more than one effect on '{phrase.Template}' yet.");

        if (phrase.Steps is not null)
        {
            RequireSupportedComposite(module, phrase);
            return;
        }

        if (!phrase.Natives.ContainsKey("csharp"))
            throw new NotImplementedException($"The C# emitter needs a native csharp block on '{phrase.Template}'.");
        RequireSupportedSignature(module, phrase);
        RequireSupportedSpecs(module, phrase);
    }

    private static void RequireSupportedComposite(ModuleModel module, PhraseModel phrase)
    {
        // A composite cannot declare a return type (PH325): steps bind results, they do not return one.
        if (phrase.Returns is not null)
            throw new NotImplementedException($"The C# emitter does not support a return type on composite '{phrase.Template}' yet.");

        RequireSupportedSignature(module, phrase);
        foreach (var step in phrase.Steps!)
        {
            if (step is BoundEnsureStep or BoundLetUpdateStep)
                continue;
            if (step is not BoundLetStep let)
                throw new NotImplementedException($"The C# emitter does not support step '{step.Source}' yet.");
            RequireSupportedArguments(phrase, let.Call);
        }

        RequireSupportedSpecs(module, phrase);
    }

    private static void RequireSupportedSignature(ModuleModel module, PhraseModel phrase)
    {
        foreach (var hole in phrase.Holes)
        {
            if (!IsSupportedType(module, hole.Type))
                throw new NotImplementedException($"The C# emitter does not support hole type '{hole.Type}' on '{phrase.Template}' yet.");
        }

        if (phrase.Returns is not null && !IsSupportedType(module, phrase.Returns))
            throw new NotImplementedException($"The C# emitter does not support return type '{phrase.Returns}' on '{phrase.Template}' yet.");
    }

    private static void RequireSupportedArguments(PhraseModel phrase, BoundCall call)
    {
        foreach (var argument in call.Arguments)
        {
            if (argument.Value is not BoundVariable)
                throw new NotImplementedException($"The C# emitter does not support the call argument in '{phrase.Template}' yet.");
        }
    }

    private static void RequireSupportedSpecs(ModuleModel module, PhraseModel phrase)
    {
        foreach (var spec in phrase.Specs)
        {
            if (spec.Outcome is not FailsModel && spec.Outcome is not SucceedsModel &&
                (spec.Outcome is not ReturnsModel returns || !IsSupportedValue(module, returns.Value)))
                throw new NotImplementedException($"The C# emitter does not support the outcome of spec '{spec.Description}' yet.");
            foreach (var given in spec.Givens)
            {
                if (!IsSupportedValue(module, given.Value))
                    throw new NotImplementedException($"The C# emitter does not support the given value of spec '{spec.Description}' yet.");
            }
        }
    }

    private static bool IsSupportedType(ModuleModel module, PType type)
    {
        if (type.IsError)
            return false;
        if (type == PType.Int || type == PType.Text || type == PType.Decimal || type == PType.Bool || type == PType.Id)
            return true;
        if (type.IsList)
            return IsSupportedType(module, type.Arguments[0]);
        if (type.IsOption)
            return IsSupportedType(module, type.Arguments[0]);
        return module.Types.Any(t => t.Name == type.Name);
    }

    private static bool IsSupportedValue(ModuleModel module, BoundExpr value) => value switch
    {
        BoundNumber => true,
        BoundText => true,
        BoundBool => true,
        BoundRecord record => record.Fields.All(field => IsSupportedValue(module, field.Value)),
        BoundList list => list.Items.All(item => IsSupportedValue(module, item)),
        _ => false,
    };

    private static string ApiProject(string module) =>
        "<Project Sdk=\"Microsoft.NET.Sdk.Web\">\n" +
        "  <PropertyGroup>\n" +
        "    <TargetFramework>net8.0</TargetFramework>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        $"    <RootNamespace>{module}</RootNamespace>\n" +
        "  </PropertyGroup>\n" +
        "  <ItemGroup>\n" +
        "    <PackageReference Include=\"FastEndpoints\" Version=\"8.3.0\" />\n" +
        "  </ItemGroup>\n" +
        "</Project>\n";

    private static string SpecsProject(string module) =>
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
        "  <PropertyGroup>\n" +
        "    <TargetFramework>net8.0</TargetFramework>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        "    <IsPackable>false</IsPackable>\n" +
        $"    <RootNamespace>{module}.Specs</RootNamespace>\n" +
        "  </PropertyGroup>\n" +
        "  <ItemGroup>\n" +
        "    <PackageReference Include=\"Microsoft.NET.Test.Sdk\" Version=\"17.11.1\" />\n" +
        "    <PackageReference Include=\"xunit\" Version=\"2.9.2\" />\n" +
        "    <PackageReference Include=\"xunit.runner.visualstudio\" Version=\"2.8.2\" />\n" +
        "  </ItemGroup>\n" +
        "  <ItemGroup>\n" +
        "    <Using Include=\"Xunit\" />\n" +
        "  </ItemGroup>\n" +
        "  <ItemGroup>\n" +
        $"    <ProjectReference Include=\"../{module}.Api/{module}.Api.csproj\" />\n" +
        "  </ItemGroup>\n" +
        "</Project>\n";

    private static string Runtime(string module) =>
        "// <auto-generated>\n" +
        "// Emitted by the Phrase C# adapter. Shared runtime support for generated code.\n" +
        "// </auto-generated>\n" +
        "using System.Diagnostics.CodeAnalysis;\n" +
        "\n" +
        $"namespace {module};\n" +
        "\n" +
        "/// <summary>Links a generated symbol back to the phrase it came from.</summary>\n" +
        "[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]\n" +
        "public sealed class PhraseAttribute(string template) : Attribute\n" +
        "{\n" +
        "    public string Template { get; } = template;\n" +
        "}\n" +
        "\n" +
        "/// <summary>A declared failure code raised by a phrase (`fails with Code`).</summary>\n" +
        "public sealed class PhraseFailure(string code) : Exception(code)\n" +
        "{\n" +
        "    public string Code { get; } = code;\n" +
        "\n" +
        "    [DoesNotReturn]\n" +
        "    public static void Raise(string code) => throw new PhraseFailure(code);\n" +
        "}\n";

    private static string Program(ModuleModel module)
    {
        var stores = PersistenceStores(module);
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# provider.\n");
        code.Append("// </auto-generated>\n");
        code.Append("using FastEndpoints;\n");
        if (stores.Count > 0)
            code.Append("using ").Append(module.Name).Append(";\n");
        code.Append('\n');
        code.Append("var builder = WebApplication.CreateBuilder(args);\n");
        code.Append("builder.Services.AddFastEndpoints();\n");
        foreach (var store in stores)
        {
            code.Append("builder.Services.AddSingleton<I").Append(CSharpIdent(store.Effect))
                .Append(", ").Append(InMemoryStoreName(store.Entity.Name)).Append(">();\n");
        }

        code.Append('\n');
        code.Append("var app = builder.Build();\n");
        code.Append("app.UseFastEndpoints();\n");
        code.Append("app.Run();\n\n");
        code.Append("public partial class Program;\n");
        return code.ToString();
    }

    // Phrase has no syntax for effect members. save, find and load are vocabulary, not declarations.
    // An effect named {Record}Store, used with that record, and whose record has an id of Text or Id,
    // is the provisional prototype store: Save replaces an item with the same id, Find, Remove(id) -> bool,
    // and List. The in-memory class locks each method. Specs and Program.cs share that class.
    private sealed record PersistenceStore(string Effect, TypeModel Entity);

    private static List<PersistenceStore> PersistenceStores(ModuleModel module)
    {
        var stores = new List<PersistenceStore>();
        foreach (var effect in module.Phrases.SelectMany(phrase => phrase.Effects).Distinct(StringComparer.Ordinal))
        {
            var entity = PersistenceEntity(module, effect);
            if (entity is not null)
                stores.Add(new PersistenceStore(effect, entity));
        }

        return stores;
    }

    private static TypeModel? PersistenceEntity(ModuleModel module, string effect)
    {
        const string suffix = "Store";
        if (!effect.EndsWith(suffix, StringComparison.Ordinal) || effect.Length == suffix.Length)
            return null;

        var entityName = effect[..^suffix.Length];
        var entity = module.Types.FirstOrDefault(type => type.Name == entityName);
        if (entity is null)
            return null;

        var id = entity.Fields.FirstOrDefault(field => field.Name == "id");
        if (id is null || (id.Type != PType.Text && id.Type != PType.Id))
            return null;

        var usedWithRecord = module.Phrases.Any(phrase =>
            phrase.Effects.Any(name => string.Equals(name, effect, StringComparison.Ordinal)) &&
            (MentionsType(phrase.Returns, entityName) || phrase.Holes.Any(hole => MentionsType(hole.Type, entityName))));
        return usedWithRecord ? entity : null;
    }

    private static bool MentionsType(PType? type, string entityName)
    {
        if (type is null)
            return false;
        return type.Name == entityName || type.Arguments.Any(argument => MentionsType(argument, entityName));
    }

    private static string InMemoryStoreName(string entityName) => CSharpIdent("InMemory" + entityName + "Store");

    private static void AppendPersistenceStore(StringBuilder code, string effect, TypeModel entity)
    {
        var typeName = CSharpIdent(entity.Name);
        var interfaceName = "I" + CSharpIdent(effect);
        var className = InMemoryStoreName(entity.Name);
        var id = CSharpFieldName(entity.Fields.First(field => field.Name == "id").Name);
        code.Append("public interface ").Append(interfaceName).Append('\n');
        code.Append("{\n");
        code.Append("    void Save(").Append(typeName).Append(" item);\n");
        code.Append("    ").Append(typeName).Append("? Find(string id);\n");
        code.Append("    bool Remove(string id);\n");
        code.Append("    IReadOnlyList<").Append(typeName).Append("> List();\n");
        code.Append("}\n\n");
        code.Append("public class ").Append(className).Append(" : ").Append(interfaceName).Append('\n');
        code.Append("{\n");
        code.Append("    private readonly List<").Append(typeName).Append("> items = new();\n");
        code.Append("    private readonly object gate = new();\n\n");
        code.Append("    public void Save(").Append(typeName).Append(" item)\n");
        code.Append("    {\n");
        code.Append("        lock (gate)\n");
        code.Append("        {\n");
        code.Append("            for (var index = 0; index < items.Count; index++)\n");
        code.Append("            {\n");
        code.Append("                if (items[index].").Append(id).Append(" == item.").Append(id).Append(")\n");
        code.Append("                {\n");
        code.Append("                    items[index] = item;\n");
        code.Append("                    return;\n");
        code.Append("                }\n");
        code.Append("            }\n\n");
        code.Append("            items.Add(item);\n");
        code.Append("        }\n");
        code.Append("    }\n\n");
        code.Append("    public ").Append(typeName).Append("? Find(string id)\n");
        code.Append("    {\n");
        code.Append("        lock (gate)\n");
        code.Append("        {\n");
        code.Append("            foreach (var item in items)\n");
        code.Append("            {\n");
        code.Append("                if (item.").Append(id).Append(" == id)\n");
        code.Append("                    return item;\n");
        code.Append("            }\n\n");
        code.Append("            return null;\n");
        code.Append("        }\n");
        code.Append("    }\n\n");
        code.Append("    public bool Remove(string id)\n");
        code.Append("    {\n");
        code.Append("        lock (gate)\n");
        code.Append("        {\n");
        code.Append("            for (var index = 0; index < items.Count; index++)\n");
        code.Append("            {\n");
        code.Append("                if (items[index].").Append(id).Append(" == id)\n");
        code.Append("                {\n");
        code.Append("                    items.RemoveAt(index);\n");
        code.Append("                    return true;\n");
        code.Append("                }\n");
        code.Append("            }\n\n");
        code.Append("            return false;\n");
        code.Append("        }\n");
        code.Append("    }\n\n");
        code.Append("    public IReadOnlyList<").Append(typeName).Append("> List()\n");
        code.Append("    {\n");
        code.Append("        lock (gate)\n");
        code.Append("            return items.ToArray();\n");
        code.Append("    }\n");
        code.Append("}\n\n");
    }

    private static string Types(ModuleModel module)
    {
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# adapter. Declared record types are generated here.\n");
        code.Append("// </auto-generated>\n");
        code.Append("namespace ").Append(module.Name).Append(";\n\n");
        foreach (var type in module.Types)
        {
            code.Append("public sealed record ").Append(CSharpIdent(type.Name)).Append('(');
            code.Append(string.Join(", ", type.Fields.Select(field => $"{CSharpType(field.Type)} {CSharpFieldName(field.Name)}")));
            code.Append(");\n");
        }
        return code.ToString();
    }

    private static string Phrases(ModuleModel module)
    {
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# adapter. Change the .avowry source, not this file.\n");
        code.Append("// Method bodies are the phrases' `native csharp` blocks, passed through unchanged.\n");
        code.Append("// </auto-generated>\n");
        if (DeclaresTask(module))
            code.Append("#nullable enable\n");
        if (module.Phrases.Any(phrase => phrase.Failures.Count > 0))
            code.Append("using System.Diagnostics.CodeAnalysis;\n\n");
        code.Append("namespace ").Append(module.Name).Append(";\n\n");
        foreach (var effect in module.Phrases.SelectMany(phrase => phrase.Effects).Distinct(StringComparer.Ordinal))
        {
            var entity = PersistenceEntity(module, effect);
            if (entity is null)
                code.Append("public interface I").Append(CSharpIdent(effect)).Append(" { }\n\n");
            else
                AppendPersistenceStore(code, effect, entity);
        }
        code.Append("public static class Phrases\n{\n");
        for (var i = 0; i < module.Phrases.Count; i++)
        {
            if (i > 0)
                code.Append('\n');
            AppendPhrase(code, module, module.Phrases[i]);
        }

        if (module.Phrases.Any(phrase => phrase.Failures.Count > 0))
        {
            code.Append("\n    [DoesNotReturn]\n");
            code.Append("    private static void Fail(string code) => PhraseFailure.Raise(code);\n");
        }
        code.Append("}\n");
        return code.ToString();
    }

    private static void AppendPhrase(StringBuilder code, ModuleModel module, PhraseModel phrase)
    {
        var method = MethodName(phrase);
        code.Append("    /// <summary>").Append(Summary(phrase)).Append("</summary>\n");
        if (phrase.Failures.Count > 0)
            code.Append("    /// <remarks>Fails with: ").Append(string.Join(", ", phrase.Failures)).Append("</remarks>\n");
        code.Append("    [Phrase(").Append(CsString(Signature(phrase))).Append(")]\n");
        code.Append("    public static ").Append(phrase.Returns is null ? "void" : CSharpType(phrase.Returns)).Append(' ').Append(method).Append('(').Append(Parameters(phrase)).Append(")\n");
        code.Append("    {\n");
        code.Append(phrase.Steps is null ? IndentNative(phrase.Natives["csharp"]) : StepsBody(module, phrase.Steps));
        code.Append("\n    }\n");
    }

    // One comment and one statement per step, matching reference/csharp CreateOrderHandler.
    private static string StepsBody(ModuleModel module, IReadOnlyList<BoundStep> steps)
    {
        var body = new StringBuilder();
        for (var i = 0; i < steps.Count; i++)
        {
            if (i > 0)
                body.Append('\n');
            body.Append("        // ").Append(steps[i].Source).Append('\n');
            body.Append("        ").Append(StepStatement(module, steps[i]));
        }

        return body.ToString();
    }

    private static string StepStatement(ModuleModel module, BoundStep step) => step switch
    {
        BoundLetStep let => $"var {CSharpIdent(let.Name)} = {CallExpression(module, let.Call)};",
        BoundLetUpdateStep update => $"var {CSharpIdent(update.Name)} = {RecordUpdateExpression(update)};",
        BoundEnsureStep ensure => EnsureStatement(ensure),
        _ => throw new NotImplementedException($"The C# emitter does not support step '{step.Source}' yet."),
    };

    private static string RecordUpdateExpression(BoundLetUpdateStep update)
    {
        var assignments = string.Join(", ", update.Fields.Select(field =>
            $"{CSharpFieldName(field.Name)} = {CSharpArgument(field.Value)}"));
        var body = assignments.Length == 0 ? "{ }" : "{ " + assignments + " }";
        return $"{CSharpArgument(update.Record)} with {body}";
    }

    private static string EnsureStatement(BoundEnsureStep ensure) =>
        $"if ({FailureCondition(ensure.Condition)}) PhraseFailure.Raise({CsString(ensure.Code)});";

    private static string FailureCondition(BoundCondition condition) => condition switch
    {
        BoundBlankCondition blank => blank.IsNotBlank
            ? $"string.IsNullOrWhiteSpace({CSharpArgument(blank.Value)})"
            : $"!string.IsNullOrWhiteSpace({CSharpArgument(blank.Value)})",
        BoundLengthAtMostCondition length => $"{CSharpArgument(length.Value)}.Length > {length.Maximum}",
        BoundComparisonCondition comparison => $"!({CSharpArgument(comparison.Left)} {comparison.Operator} {CSharpArgument(comparison.Right)})",
        _ => throw new NotImplementedException($"The C# emitter does not support condition '{condition.GetType().Name}' yet."),
    };

    private static string CallExpression(ModuleModel module, BoundCall call, IReadOnlyList<string>? availableEffects = null)
    {
        var target = module.Phrase(call.PhraseKey);
        var effects = target.Effects.Select(effect =>
        {
            if (availableEffects is null || !availableEffects.Contains(effect, StringComparer.Ordinal))
                throw new NotImplementedException($"The C# emitter cannot supply effect '{effect}' to '{target.Template}'.");
            return CSharpIdent(effect.ToLowerInvariant());
        });
        var arguments = string.Join(", ", effects.Concat(call.Arguments.Select(argument => CSharpArgument(argument.Value))));
        return $"Phrases.{MethodName(target)}({arguments})";
    }

    private static string CSharpArgument(BoundExpr value) => value switch
    {
        BoundVariable variable => CSharpIdent(variable.Name),
        BoundField field => field.Target switch
        {
            BoundVariable variable => $"{CSharpIdent(variable.Name)}.{CSharpFieldName(field.Name)}",
            BoundField nested => $"{CSharpArgument(nested)}.{CSharpFieldName(field.Name)}",
            _ => throw new NotImplementedException($"The C# emitter does not support call argument '{value.GetType().Name}' yet."),
        },
        _ => CSharpValue(value),
    };

    private static string EndpointResponseValue(BoundExpr value) =>
        value.Type.IsList && value is BoundVariable or BoundField
            ? CSharpArgument(value) + ".ToList()"
            : CSharpValue(value);

    private static string EndpointFile(ModuleModel module, EndpointModel endpoint)
    {
        var effects = EndpointEffects(module, endpoint);
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# provider from endpoint ").Append(endpoint.Name).Append(" in ").Append(module.Name).Append(".avowry.\n");
        code.Append("// </auto-generated>\n");
        code.Append("using FastEndpoints;\n\n");
        code.Append("namespace ").Append(module.Name).Append(";\n\n");
        code.Append("public sealed record ").Append(EndpointRequestName(endpoint.Name)).Append('(');
        code.Append(string.Join(", ", endpoint.Input.Select(field => $"{EndpointFieldType(field.Type)} {CSharpFieldName(field.Name)}")));
        code.Append(");\n\n");
        if (endpoint.OutputType is not null)
        {
            code.Append("public sealed record ").Append(EndpointResponseName(endpoint.Name)).Append('(');
            code.Append(string.Join(", ", endpoint.Output.Select(field => $"{EndpointFieldType(field.Type)} {CSharpFieldName(field.Name)}")));
            code.Append(");\n\n");
        }

        code.Append("/// <summary>Pure handler: the endpoint's steps, one line per step.</summary>\n");
        code.Append("[Phrase(\"endpoint ").Append(endpoint.Name).Append(" : ").Append(endpoint.Verb).Append(' ').Append(endpoint.Route).Append("\")]\n");
        code.Append("public static class ").Append(EndpointHandlerName(endpoint.Name)).Append("\n{");
        code.Append("    public static ").Append(endpoint.OutputType is null ? "void" : EndpointResponseName(endpoint.Name)).Append(' ');
        code.Append("Handle(").Append(EndpointRequestName(endpoint.Name)).Append(" input");
        foreach (var effect in effects)
            code.Append(", I").Append(CSharpIdent(effect)).Append(' ').Append(CSharpIdent(effect.ToLowerInvariant()));
        code.Append(")\n");
        code.Append("    {\n");
        foreach (var step in endpoint.Steps)
        {
            var statement = HandlerStep(module, endpoint, step);
            if (!string.IsNullOrEmpty(statement))
            {
                code.Append("        // ").Append(step.Source).Append('\n');
                code.Append("        ").Append(statement).Append('\n');
            }
        }
        code.Append("    }\n");
        code.Append("}\n\n");

        code.Append("/// <summary>HTTP binding. Declared failures map to 422 with the code as the error.</summary>\n");
        code.Append("public sealed class ").Append(endpoint.Name).Append("Endpoint");
        if (effects.Count > 0)
        {
            code.Append('(');
            code.Append(string.Join(", ", effects.Select(effect => $"I{CSharpIdent(effect)} {CSharpIdent(effect.ToLowerInvariant())}")));
            code.Append(')');
        }
        code.Append(" : Endpoint<").Append(EndpointRequestName(endpoint.Name));
        if (endpoint.OutputType is not null)
            code.Append(", ").Append(EndpointResponseName(endpoint.Name));
        code.Append(">\n");
        code.Append("{\n");
        code.Append("    public override void Configure()\n");
        code.Append("    {\n");
        code.Append("        ").Append(endpoint.Verb switch
        {
            "GET" => "Get",
            "POST" => "Post",
            "PUT" => "Put",
            "PATCH" => "Patch",
            "DELETE" => "Delete",
            _ => endpoint.Verb.ToLowerInvariant(),
        }).Append("(\"").Append(endpoint.Route).Append("\");\n");
        code.Append("        AllowAnonymous();\n");
        code.Append("    }\n\n");
        code.Append("    public override async ").Append(AsyncTaskType(module)).Append(" HandleAsync(").Append(EndpointRequestName(endpoint.Name)).Append(" req, CancellationToken ct)\n");
        code.Append("    {\n");
        code.Append("        try\n");
        code.Append("        {\n");
        if (endpoint.OutputType is null)
        {
            // No response record: FastEndpoints.Endpoint<TRequest> and Send.OkAsync(ct), HTTP 200 with an empty body.
            code.Append("            ").Append(EndpointHandlerName(endpoint.Name)).Append(".Handle(req");
            foreach (var effect in effects)
                code.Append(", ").Append(CSharpIdent(effect.ToLowerInvariant()));
            code.Append(");\n");
            code.Append("            await Send.OkAsync(ct);\n");
        }
        else
        {
            code.Append("            await Send.OkAsync(").Append(EndpointHandlerName(endpoint.Name)).Append(".Handle(req");
            foreach (var effect in effects)
                code.Append(", ").Append(CSharpIdent(effect.ToLowerInvariant()));
            code.Append("), ct);\n");
        }
        code.Append("        }\n");
        code.Append("        catch (PhraseFailure failure)\n");
        code.Append("        {\n");
        code.Append("            AddError(failure.Code);\n");
        code.Append("            await Send.ErrorsAsync(422, ct);\n");
        code.Append("        }\n");
        code.Append("    }\n");
        code.Append("}\n");
        return code.ToString();
    }

    private static IReadOnlyList<string> EndpointEffects(ModuleModel module, EndpointModel endpoint) =>
        endpoint.Steps.SelectMany(step => step switch
            {
                BoundCallStep call => module.Phrase(call.Call.PhraseKey).Effects,
                BoundLetStep let => module.Phrase(let.Call.PhraseKey).Effects,
                _ => [],
            })
            .Distinct(StringComparer.Ordinal)
            .ToArray();

    private static string HandlerStep(ModuleModel module, EndpointModel endpoint, BoundStep step) => step switch
    {
        BoundCallStep call => $"{CallExpression(module, call.Call, EndpointEffects(module, endpoint))};",
        BoundLetStep let => $"var {CSharpIdent(let.Name)} = {CallExpression(module, let.Call, EndpointEffects(module, endpoint))};",
        BoundLetUpdateStep update => $"var {CSharpIdent(update.Name)} = {RecordUpdateExpression(update)};",
        BoundRespondStep respond => $"return new {EndpointResponseName(endpoint.Name)}({string.Join(", ", respond.Value.Fields.Select(field => $"{CSharpFieldName(field.Name)}: {EndpointResponseValue(field.Value)}"))});",
        BoundFailStep fail => $"PhraseFailure.Raise({CsString(fail.Code)});",
        BoundEnsureStep ensure => EnsureStatement(ensure),
        _ => string.Empty,
    };

    private static string EndpointSpecs(ModuleModel module, EndpointModel endpoint)
    {
        var effects = EndpointEffects(module, endpoint);
        if (effects.Count == 0)
            return EffectFreeEndpointSpecs(module, endpoint);
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# provider from the `spec` blocks on endpoint ").Append(endpoint.Name).Append(" in ").Append(module.Name).Append(".avowry.\n");
        code.Append("// </auto-generated>\n");
        code.Append("using FastEndpoints;\n");
        code.Append("using ").Append(module.Name).Append(";\n\n");
        code.Append("namespace ").Append(module.Name).Append(".Specs;\n\n");
        code.Append("public class ").Append(EndpointSpecsName(endpoint.Name)).Append("\n{");
        foreach (var spec in endpoint.Specs)
        {
            code.Append("\n    [Fact(DisplayName = ").Append(CsString($"{endpoint.Name}: {spec.Description}")).Append(")]\n");
            code.Append("    public async ").Append(AsyncTaskType(module)).Append(' ').Append(EndpointSpecMethodName(endpoint.Name, spec.Description)).Append("()\n");
            code.Append("    {\n");
            code.Append("        // Given input ").Append(CSharpValue(spec.Input)).Append('\n');
            code.Append("        var input = ").Append(CSharpValue(spec.Input)).Append(";\n");
            foreach (var effect in effects)
            {
                var entity = PersistenceEntity(module, effect);
                code.Append("        var ").Append(CSharpIdent(effect.ToLowerInvariant())).Append(" = new ")
                    .Append(entity is null ? CSharpIdent(effect) + "Double" : InMemoryStoreName(entity.Name)).Append("();\n");
                if (entity is not null && spec.Outcome is RespondsModel expected)
                {
                    foreach (var item in ValuesOfType(expected.Value, entity.Name))
                        code.Append("        ").Append(CSharpIdent(effect.ToLowerInvariant())).Append(".Save(").Append(CSharpValue(item)).Append(");\n");
                }
                else if (entity is not null && spec.Outcome is SucceedsModel && KnownIdSeed(entity, spec.Input) is { } seed)
                {
                    // A no-output success has no response record to copy. The input id is the known item.
                    code.Append("        ").Append(CSharpIdent(effect.ToLowerInvariant())).Append(".Save(").Append(seed).Append(");\n");
                }
            }
            code.Append("        // When handled\n");
            code.Append("        var endpoint = Factory.Create<").Append(endpoint.Name).Append("Endpoint>(context => context.Response.Body = new MemoryStream()");
            foreach (var effect in effects)
                code.Append(", ").Append(CSharpIdent(effect.ToLowerInvariant()));
            code.Append(");\n");
            code.Append("        await endpoint.HandleAsync(input, CancellationToken.None);\n");
            switch (spec.Outcome)
            {
                case RespondsModel responds:
                    code.Append("        // Then responds ").Append(CSharpValue(responds.Value)).Append('\n');
                    code.Append("        Assert.Equal(200, endpoint.HttpContext.Response.StatusCode);\n");
                    AppendAssertion(code, spec.Description, CSharpAssertionValue(responds.Value), "endpoint.Response");
                    break;
                case FailsModel fails:
                    code.Append("        // Then fails with ").Append(fails.Code).Append('\n');
                    code.Append("        Assert.Equal(422, endpoint.HttpContext.Response.StatusCode);\n");
                    code.Append("        endpoint.HttpContext.Response.Body.Position = 0;\n");
                    code.Append("        var body = await new StreamReader(endpoint.HttpContext.Response.Body).ReadToEndAsync();\n");
                    code.Append("        Assert.Contains(").Append(CsString(fails.Code)).Append(", body, StringComparison.Ordinal);\n");
                    break;
                case SucceedsModel:
                    code.Append("        // Then succeeds\n");
                    code.Append("        Assert.Equal(200, endpoint.HttpContext.Response.StatusCode);\n");
                    break;
            }
            code.Append("    }\n");
        }
        AppendAssertionHelper(code);
        code.Append("}\n");
        return code.ToString();
    }

    private static string EffectFreeEndpointSpecs(ModuleModel module, EndpointModel endpoint)
    {
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# provider from the `spec` blocks on endpoint ").Append(endpoint.Name).Append(" in ").Append(module.Name).Append(".avowry.\n");
        code.Append("// </auto-generated>\n");
        code.Append("namespace ").Append(module.Name).Append(".Specs;\n\n");
        code.Append("public class ").Append(EndpointSpecsName(endpoint.Name)).Append("\n{");
        foreach (var spec in endpoint.Specs)
        {
            code.Append("\n    [Fact(DisplayName = ").Append(CsString($"{endpoint.Name}: {spec.Description}")).Append(")]\n");
            code.Append("    public void ").Append(EndpointSpecMethodName(endpoint.Name, spec.Description)).Append("()\n");
            code.Append("    {\n");
            code.Append("        // Given input ").Append(CSharpValue(spec.Input)).Append('\n');
            code.Append("        var input = ").Append(CSharpValue(spec.Input)).Append(";\n");
            code.Append("        // When handled\n");
            switch (spec.Outcome)
            {
                case RespondsModel responds:
                    code.Append("        var response = ").Append(EndpointHandlerName(endpoint.Name)).Append(".Handle(input);\n");
                    code.Append("        // Then responds ").Append(CSharpValue(responds.Value)).Append('\n');
                    AppendAssertion(code, spec.Description, CSharpAssertionValue(responds.Value), "response");
                    break;
                case FailsModel fails:
                    code.Append("        var failure = Assert.Throws<PhraseFailure>(() => ").Append(EndpointHandlerName(endpoint.Name)).Append(".Handle(input));\n");
                    code.Append("        // Then fails with ").Append(fails.Code).Append('\n');
                    AppendAssertion(code, spec.Description, CsString(fails.Code), "failure.Code");
                    break;
                case SucceedsModel:
                    code.Append("        var response = ").Append(EndpointHandlerName(endpoint.Name)).Append(".Handle(input);\n");
                    code.Append("        // Then succeeds\n");
                    break;
            }
            code.Append("    }\n");
        }
        AppendAssertionHelper(code);
        code.Append("}\n");
        return code.ToString();
    }

    // `then succeeds` carries no record. Seed one item whose id is the spec input's id
    // so a known id is present and an unknown id (a failure spec) is not.
    private static string? KnownIdSeed(TypeModel entity, BoundRecord input)
    {
        var supplied = input.Fields.FirstOrDefault(field => field.Name == "id");
        if (supplied?.Value is not BoundText || entity.Fields.All(field => field.Name != "id"))
            return null;

        var arguments = entity.Fields.Select(field =>
            $"{CSharpFieldName(field.Name)}: {(field.Name == "id" ? CSharpValue(supplied.Value) : StoredDefault(field.Type))}");
        return $"new {CSharpIdent(entity.Name)}({string.Join(", ", arguments)})";
    }

    private static string StoredDefault(PType type)
    {
        if (type == PType.Text || type == PType.Id)
            return "\"\"";
        if (type == PType.Bool)
            return "false";
        throw new NotImplementedException($"The C# emitter cannot seed a stored field of type '{type}' yet.");
    }

    private static IEnumerable<BoundExpr> ValuesOfType(BoundExpr value, string typeName)
    {
        if (value.Type.Name == typeName)
            yield return value;
        if (value is BoundRecord record)
        {
            foreach (var nested in record.Fields.SelectMany(field => ValuesOfType(field.Value, typeName)))
                yield return nested;
        }
        else if (value is BoundList list)
        {
            foreach (var nested in list.Items.SelectMany(item => ValuesOfType(item, typeName)))
                yield return nested;
        }
    }

    private static string EndpointFileName(string endpoint) => endpoint + ".cs";
    private static bool DeclaresTask(ModuleModel module) =>
        module.Types.Any(type => type.Name.Equals("Task", StringComparison.Ordinal));
    private static string AsyncTaskType(ModuleModel module) =>
        DeclaresTask(module) ? "global::System.Threading.Tasks.Task" : "Task";
    private static string EndpointSpecsFileName(string endpoint) => endpoint + "Specs.cs";
    private static string EndpointSpecsName(string endpoint) => endpoint + "Specs";
    private static string EndpointRequestName(string endpoint) => endpoint + "Request";
    private static string EndpointResponseName(string endpoint) => endpoint + "Response";
    private static string EndpointHandlerName(string endpoint) => endpoint + "Handler";
    private static string EndpointSpecMethodName(string endpoint, string description) =>
        endpoint + "__" + Slug(description);

    private static string EndpointFieldType(PType type)
    {
        if (type == PType.Int) return "int";
        if (type == PType.Text || type == PType.Id) return "string";
        if (type == PType.Decimal) return "decimal";
        if (type == PType.Bool) return "bool";
        if (type.IsList) return $"List<{EndpointFieldType(type.Arguments[0])}>";
        if (type.IsOption) return $"{EndpointFieldType(type.Arguments[0])}?";
        if (type.IsEndpointShape)
        {
            var name = type.Name.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];
            return name + (type.Name.EndsWith(".Output", StringComparison.Ordinal) ? "Response" : "Request");
        }
        return CSharpIdent(type.Name);
    }

    private static string PhraseSpecs(ModuleModel module)
    {
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# adapter from the `spec` blocks on phrases.\n");
        code.Append("// </auto-generated>\n");
        code.Append("using ").Append(module.Name).Append(";\n\n");
        code.Append("namespace ").Append(module.Name).Append(".Specs;\n\n");
        code.Append("public class PhraseSpecs\n{\n");
        var first = true;
        foreach (var phrase in module.Phrases)
        {
            foreach (var spec in phrase.Specs)
            {
                if (!first)
                    code.Append('\n');
                first = false;
                AppendSpec(code, phrase, spec);
            }
        }

        foreach (var effect in module.Phrases.SelectMany(phrase => phrase.Effects).Distinct(StringComparer.Ordinal))
        {
            var entity = PersistenceEntity(module, effect);
            code.Append("\n    private sealed class ").Append(CSharpIdent(effect)).Append("Double : ");
            if (entity is null)
                code.Append('I').Append(CSharpIdent(effect));
            else
                code.Append(InMemoryStoreName(entity.Name));
            code.Append(" { }\n");
        }
        AppendAssertionHelper(code);
        code.Append("}\n");
        return code.ToString();
    }

    private static void AppendSpec(StringBuilder code, PhraseModel phrase, PhraseSpecModel spec)
    {
        code.Append("    [Fact(DisplayName = ").Append(CsString($"{Summary(phrase)}: {spec.Description}")).Append(")]\n");
        code.Append("    public void ").Append(SpecMethodName(phrase, spec.Description)).Append("()\n");
        code.Append("    {\n");
        var index = 0;
        foreach (var given in spec.Givens)
        {
            code.Append("        // ").Append(Capitalize(spec.Lines[index++])).Append('\n');
            code.Append("        ").Append(CSharpType(given.Hole.Type)).Append(" ").Append(CSharpIdent(given.Hole.Name)).Append(" = ").Append(CSharpValue(given.Value)).Append(";\n");
        }

        code.Append("        // ").Append(Capitalize(spec.Lines[index++])).Append('\n');
        var fails = spec.Outcome as FailsModel;
        var succeeds = spec.Outcome is SucceedsModel;
        code.Append(fails is not null ? "        var failure = Assert.Throws<PhraseFailure>(() => Phrases." : succeeds ? "        Phrases." : "        var result = Phrases.").Append(MethodName(phrase)).Append('(');
        code.Append(string.Join(", ", phrase.Effects.Select(effect => $"new {CSharpIdent(effect)}Double()").Concat(phrase.Holes.Select(hole => CSharpIdent(hole.Name)))));
        code.Append(fails is null ? ");\n" : "));\n");
        code.Append("        // ").Append(Capitalize(spec.Lines[index])).Append('\n');
        if (spec.Outcome is ReturnsModel returns)
            AppendAssertion(code, spec.Description, CSharpAssertionValue(returns.Value), "result");
        else if (fails is not null)
            AppendAssertion(code, spec.Description, CsString(fails.Code), "failure.Code");
        code.Append("    }\n");
    }

    private static void AppendAssertion(StringBuilder code, string description, string expected, string actual)
    {
        code.Append("        AssertSpecEqual(").Append(CsString(description)).Append(", ").Append(expected).Append(", ").Append(actual).Append(");\n");
    }

    private static void AppendAssertionHelper(StringBuilder code)
    {
        code.Append("\n    private static void AssertSpecEqual(string spec, object expected, object actual)\n");
        code.Append("    {\n");
        code.Append("        try\n");
        code.Append("        {\n");
        code.Append("            Assert.Equivalent(expected, actual, strict: true);\n");
        code.Append("        }\n");
        code.Append("        catch (Xunit.Sdk.XunitException)\n");
        code.Append("        {\n");
        code.Append("            throw new Xunit.Sdk.XunitException($\"Spec \\\"{spec}\\\" expected {Format(expected)} but got {Format(actual)}.\");\n");
        code.Append("        }\n");
        code.Append("    }\n");
        code.Append("\n    private static string Format(object value) => value is System.Collections.IEnumerable items and not string\n");
        code.Append("        ? \"[\" + string.Join(\", \", items.Cast<object>().Select(Format)) + \"]\"\n");
        code.Append("        : value?.ToString() ?? \"null\";\n");
    }

    private static string Signature(PhraseModel phrase) =>
        phrase.Returns is null ? phrase.Template : $"{phrase.Template} -> {phrase.Returns}";

    private static string Summary(PhraseModel phrase) =>
        string.Join(" ", phrase.Parts.Select(part => part.IsHole ? "{" + part.Text + "}" : part.Text));

    private static string MethodName(PhraseModel phrase) =>
        string.Concat(phrase.Parts.Select(part => Pascal(part.Text)));

    private static string SpecMethodName(PhraseModel phrase, string description)
    {
        var template = string.Join("_", phrase.Parts.Select(part => part.Text.ToLowerInvariant()));
        var name = template + "__" + Slug(description);
        return char.ToUpperInvariant(name[0]) + name[1..];
    }

    private static string Parameters(PhraseModel phrase) =>
        string.Join(", ", phrase.Effects.Select(effect => $"I{CSharpIdent(effect)} {CSharpIdent(effect.ToLowerInvariant())}")
            .Concat(phrase.Holes.Select(hole => $"{CSharpType(hole.Type)} {CSharpIdent(hole.Name)}")));

    private static string Pascal(string word) =>
        word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..];

    private static string CSharpFieldName(string name) =>
        CSharpIdent(name.Length == 0 ? name : char.ToUpperInvariant(name[0]) + name[1..]);

    // Phrase type names and Pascal-cased fields never spell a reserved word exactly.
    // Match keywords ignoring case so Event and Class still get the @ prefix.
    private static string CSharpIdent(string name) =>
        CSharpKeywords.Contains(name) ? "@" + name : name;

    private static readonly HashSet<string> CSharpKeywords = new(StringComparer.OrdinalIgnoreCase)
    {
        "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char",
        "checked", "class", "const", "continue", "decimal", "default", "delegate",
        "do", "double", "else", "enum", "event", "explicit", "extern", "false",
        "finally", "fixed", "float", "for", "foreach", "goto", "if", "implicit",
        "in", "int", "interface", "internal", "is", "lock", "long", "namespace",
        "new", "null", "object", "operator", "out", "override", "params", "private",
        "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
        "short", "sizeof", "stackalloc", "static", "string", "struct", "switch",
        "this", "throw", "true", "try", "typeof", "uint", "ulong", "unchecked",
        "unsafe", "ushort", "using", "virtual", "void", "volatile", "while",
    };

    private static string CSharpType(PType type)
    {
        if (type == PType.Int) return "int";
        if (type == PType.Text || type == PType.Id) return "string";
        if (type == PType.Decimal) return "decimal";
        if (type == PType.Bool) return "bool";
        if (type.IsList) return $"IReadOnlyList<{CSharpType(type.Arguments[0])}>";
        if (type.IsOption) return $"{CSharpType(type.Arguments[0])}?";
        if (type.IsEndpointShape)
        {
            var name = type.Name.Split('.', StringSplitOptions.RemoveEmptyEntries)[0];
            return name + (type.Name.EndsWith(".Output", StringComparison.Ordinal) ? "Response" : "Request");
        }
        return CSharpIdent(type.Name);
    }

    private static string Slug(string text)
    {
        var chars = text.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray();
        return string.Join("_", new string(chars).Split('_', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NumberText(BoundExpr value) =>
        value is BoundNumber number ? number.Text : throw new InvalidOperationException($"Expected an Int literal, found {value.GetType().Name}.");

    private static string CSharpValue(BoundExpr value) => value switch
    {
        BoundVariable variable => CSharpIdent(variable.Name),
        BoundField field => $"{CSharpValue(field.Target)}.{CSharpFieldName(field.Name)}",
        BoundText text => CsString(text.Value),
        BoundNumber number => number.Text,
        BoundBool boolean => boolean.Value ? "true" : "false",
        BoundNone none => "null",
        BoundList list => $"[{string.Join(", ", list.Items.Select(CSharpValue))}]",
        BoundRecord record => $"new {CSharpType(record.Type)}({string.Join(", ", record.Fields.Select(field => $"{CSharpFieldName(field.Name)}: {CSharpValue(field.Value)}"))})",
        _ => throw new InvalidOperationException($"Unsupported C# expression kind {value.GetType().Name}.")
    };

    private static string CSharpAssertionValue(BoundExpr value) => value switch
    {
        BoundList list => $"new[] {{ {string.Join(", ", list.Items.Select(CSharpValue))} }}",
        _ => CSharpValue(value),
    };

    private static string Capitalize(string line) =>
        line.Length == 0 ? line : char.ToUpperInvariant(line[0]) + line[1..];

    private static string CsString(string value) =>
        "\"" + value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal) + "\"";

    private static string IndentNative(string code)
    {
        var text = code.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n');
        var builder = new StringBuilder();
        foreach (var line in text.Split('\n'))
        {
            if (builder.Length > 0)
                builder.Append('\n');
            if (line.Length > 0)
                builder.Append("        ").Append(line);
        }

        return builder.ToString();
    }
}
