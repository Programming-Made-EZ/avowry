using System.Text;
using Phrase.Compiler;
using Phrase.Compiler.Semantics;

namespace Phrase.Providers.CSharp;

/// <summary>
/// Emits a C# API project and an xUnit specs project for a leaf phrase.
/// The hand-written shape to match is reference/csharp. This slice emits one
/// Int-returning leaf and its Given/When/Then specs; records, failures, and
/// endpoints are left for later slices.
/// </summary>
public sealed class CSharpProvider : ITargetProvider
{
    public string Target => "csharp";

    public CompilationOutput Emit(ModuleModel module)
    {
        ArgumentNullException.ThrowIfNull(module);
        if (module.Types.Count > 0)
            throw new NotImplementedException("The C# emitter does not support records yet.");
        if (module.Endpoints.Count > 0)
            throw new NotImplementedException("The C# emitter does not support endpoints yet.");
        if (module.Name.Length == 0)
            throw new NotImplementedException("The C# emitter needs a module name.");

        foreach (var phrase in module.Phrases)
            RequireSupportedPhrase(phrase);

        var api = module.Name + ".Api";
        var specs = module.Name + ".Specs";
        var files = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            [$"{api}/{module.Name}.Api.csproj"] = ApiProject(module.Name),
            [$"{api}/PhraseRuntime.cs"] = Runtime(module.Name),
            [$"{api}/Phrases.cs"] = Phrases(module),
            [$"{specs}/{module.Name}.Specs.csproj"] = SpecsProject(module.Name),
            [$"{specs}/PhraseSpecs.cs"] = PhraseSpecs(module),
        };
        return new CompilationOutput(files);
    }

    private static void RequireSupportedPhrase(PhraseModel phrase)
    {
        if (phrase.Steps is not null)
            throw new NotImplementedException($"The C# emitter does not support composite phrase '{phrase.Template}' yet.");
        if (phrase.Failures.Count > 0)
            throw new NotImplementedException($"The C# emitter does not support failure codes on '{phrase.Template}' yet.");
        if (phrase.Effects.Count > 0)
            throw new NotImplementedException($"The C# emitter does not support effects on '{phrase.Template}' yet.");
        if (!phrase.Natives.ContainsKey("csharp"))
            throw new NotImplementedException($"The C# emitter needs a native csharp block on '{phrase.Template}'.");
        if (phrase.Returns is null || !phrase.Returns.Equals(PType.Int))
            throw new NotImplementedException($"The C# emitter does not support return type '{phrase.Returns?.ToString() ?? "nothing"}' on '{phrase.Template}' yet.");

        foreach (var hole in phrase.Holes)
        {
            if (!hole.Type.Equals(PType.Int))
                throw new NotImplementedException($"The C# emitter does not support hole type '{hole.Type}' on '{phrase.Template}' yet.");
        }

        foreach (var spec in phrase.Specs)
        {
            if (spec.Outcome is not ReturnsModel returns || returns.Value is not BoundNumber number || !number.Type.Equals(PType.Int))
                throw new NotImplementedException($"The C# emitter does not support the outcome of spec '{spec.Description}' yet.");
            foreach (var given in spec.Givens)
            {
                if (given.Value is not BoundNumber givenNumber || !givenNumber.Type.Equals(PType.Int))
                    throw new NotImplementedException($"The C# emitter does not support the given value of spec '{spec.Description}' yet.");
            }
        }
    }

    private static string ApiProject(string module) =>
        "<Project Sdk=\"Microsoft.NET.Sdk\">\n" +
        "  <PropertyGroup>\n" +
        "    <TargetFramework>net8.0</TargetFramework>\n" +
        "    <Nullable>enable</Nullable>\n" +
        "    <ImplicitUsings>enable</ImplicitUsings>\n" +
        $"    <RootNamespace>{module}</RootNamespace>\n" +
        "  </PropertyGroup>\n" +
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
        "// Emitted by the Phrase C# provider. Shared runtime support for generated code.\n" +
        "// </auto-generated>\n" +
        $"namespace {module};\n" +
        "\n" +
        "/// <summary>Links a generated symbol back to the phrase it came from.</summary>\n" +
        "[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]\n" +
        "public sealed class PhraseAttribute(string template) : Attribute\n" +
        "{\n" +
        "    public string Template { get; } = template;\n" +
        "}\n";

    private static string Phrases(ModuleModel module)
    {
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# provider. Change the .phrase source, not this file.\n");
        code.Append("// Method bodies are the phrases' `native csharp` blocks, passed through unchanged.\n");
        code.Append("// </auto-generated>\n");
        code.Append("namespace ").Append(module.Name).Append(";\n\n");
        code.Append("public static class Phrases\n{\n");
        for (var i = 0; i < module.Phrases.Count; i++)
        {
            if (i > 0)
                code.Append('\n');
            AppendPhrase(code, module.Phrases[i]);
        }

        code.Append("}\n");
        return code.ToString();
    }

    private static void AppendPhrase(StringBuilder code, PhraseModel phrase)
    {
        var method = MethodName(phrase);
        code.Append("    /// <summary>").Append(Summary(phrase)).Append("</summary>\n");
        code.Append("    [Phrase(").Append(CsString(Signature(phrase))).Append(")]\n");
        code.Append("    public static int ").Append(method).Append('(').Append(Parameters(phrase)).Append(")\n");
        code.Append("    {\n");
        code.Append(IndentNative(phrase.Natives["csharp"]));
        code.Append("\n    }\n");
    }

    private static string PhraseSpecs(ModuleModel module)
    {
        var code = new StringBuilder();
        code.Append("// <auto-generated>\n");
        code.Append("// Emitted by the Phrase C# provider from the `spec` blocks on phrases.\n");
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
            code.Append("        int ").Append(given.Hole.Name).Append(" = ").Append(NumberText(given.Value)).Append(";\n");
        }

        code.Append("        // ").Append(Capitalize(spec.Lines[index++])).Append('\n');
        code.Append("        var result = Phrases.").Append(MethodName(phrase)).Append('(');
        code.Append(string.Join(", ", phrase.Holes.Select(hole => hole.Name)));
        code.Append(");\n");
        code.Append("        // ").Append(Capitalize(spec.Lines[index])).Append('\n');
        code.Append("        Assert.Equal(").Append(NumberText(((ReturnsModel)spec.Outcome).Value)).Append(", result);\n");
        code.Append("    }\n");
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
        string.Join(", ", phrase.Holes.Select(hole => "int " + hole.Name));

    private static string Pascal(string word) =>
        word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..];

    private static string Slug(string text)
    {
        var chars = text.Trim().ToLowerInvariant().Select(ch => char.IsLetterOrDigit(ch) ? ch : '_').ToArray();
        return string.Join("_", new string(chars).Split('_', StringSplitOptions.RemoveEmptyEntries));
    }

    private static string NumberText(BoundExpr value) =>
        value is BoundNumber number ? number.Text : throw new InvalidOperationException($"Expected an Int literal, found {value.GetType().Name}.");

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
