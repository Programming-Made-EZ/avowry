namespace Phrase.Compiler;

/// <summary>A compiler message tied to a source position. Codes are stable (PH###) so tools and LLMs can key on them.</summary>
public sealed record Diagnostic(string Code, string Message, int Line, int Column)
{
    public override string ToString() => $"{Code} ({Line}:{Column}) {Message}";
}
