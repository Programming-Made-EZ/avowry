using Phrase.Compiler.Lexing;

namespace Phrase.Compiler.Tests.Lexing;

// Feature: Robustness on malformed input
//   LLM-written source will often be slightly wrong. The lexer must always return
//   a usable token stream plus diagnostics, never throw, and never unbalance layout.
public class LexerRobustnessTests
{
    private static readonly string Orders =
        File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "Fixtures", "orders.phrase"));

    private const string Alphabet = " \t\n\"{}[]<>-#\\:,.=/+aZ9";

    public static TheoryData<int> Seeds => new() { 1, 2, 3, 4, 5, 6, 7, 8 };

    [Theory]
    [MemberData(nameof(Seeds))]
    public void Mutated_programs_lex_without_throwing_and_with_balanced_layout(int seed)
    {
        // Given the orders fixture with random single-character edits (deterministic per seed)
        var random = new Random(seed);
        for (var round = 0; round < 250; round++)
        {
            var source = Mutate(Orders, random, edits: random.Next(1, 6));

            // When it is lexed
            var tokens = Lexer.Lex(source).Tokens;

            // Then every INDENT is matched by a DEDENT
            Assert.Equal(
                tokens.Count(t => t.Kind == TokenKind.Indent),
                tokens.Count(t => t.Kind == TokenKind.Dedent));

            // And the stream ends with exactly one end of file
            Assert.Equal(TokenKind.EndOfFile, tokens[^1].Kind);
            Assert.Single(tokens, t => t.Kind == TokenKind.EndOfFile);
        }
    }

    private static string Mutate(string source, Random random, int edits)
    {
        var chars = source.ToList();
        for (var e = 0; e < edits && chars.Count > 0; e++)
        {
            var at = random.Next(chars.Count);
            switch (random.Next(3))
            {
                case 0: chars.RemoveAt(at); break;
                case 1: chars.Insert(at, Alphabet[random.Next(Alphabet.Length)]); break;
                default: chars[at] = Alphabet[random.Next(Alphabet.Length)]; break;
            }
        }
        return new string([.. chars]);
    }
}
