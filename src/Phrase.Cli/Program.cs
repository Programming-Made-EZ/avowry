using Phrase.Adapters.CSharp;
using Phrase.Compiler;

const string Usage = "Usage: phrase build FILE.phrase -o OUT_DIR [--target csharp]";

if (!TryParse(args, out var file, out var output))
{
    Console.Error.WriteLine(Usage);
    return 2;
}

if (!File.Exists(file))
{
    Console.Error.WriteLine($"No such file: {file}");
    return 2;
}

try
{
    var compiled = PhraseCompiler.Compile(File.ReadAllText(file), new CSharpProvider(), file);
    compiled.WriteTo(output);
    return 0;
}
catch (CompilationFailedException exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}

static bool TryParse(string[] args, out string file, out string output)
{
    file = "";
    output = "";
    if (args.Length == 0 || args[0] != "build")
        return false;

    string? phraseFile = null;
    string? outDir = null;

    for (var i = 1; i < args.Length; i++)
    {
        var arg = args[i];
        if (arg == "-o")
        {
            if (i + 1 >= args.Length)
                return false;
            outDir = args[++i];
        }
        else if (arg == "--target")
        {
            if (i + 1 >= args.Length || args[++i] != "csharp")
                return false;
        }
        else if (arg.StartsWith('-'))
        {
            return false;
        }
        else if (phraseFile is not null)
        {
            return false;
        }
        else
        {
            phraseFile = arg;
        }
    }

    if (phraseFile is null || outDir is null)
        return false;

    file = phraseFile;
    output = outDir;
    return true;
}
