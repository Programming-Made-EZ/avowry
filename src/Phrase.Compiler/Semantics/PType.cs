namespace Phrase.Compiler.Semantics;

/// <summary>
/// A resolved type. Equality is structural (by canonical text), so <c>List&lt;LineItem&gt;</c> equals <c>List&lt;LineItem&gt;</c>.
/// Endpoint request and response shapes are named <c>Endpoint.Input</c> and <c>Endpoint.Output</c>; user types cannot contain a dot.
/// </summary>
public sealed record PType(string Name, IReadOnlyList<PType> Arguments)
{
    public static readonly PType Text = new("Text", []);
    public static readonly PType Int = new("Int", []);
    public static readonly PType Decimal = new("Decimal", []);
    public static readonly PType Bool = new("Bool", []);
    public static readonly PType Id = new("Id", []);

    /// <summary>Stands in for a type that failed to resolve. Compatible with everything, so one error does not cascade.</summary>
    public static readonly PType Error = new("?", []);

    /// <summary>Core type names and how many type arguments each takes.</summary>
    public static readonly IReadOnlyDictionary<string, int> Core = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["Text"] = 0, ["Int"] = 0, ["Decimal"] = 0, ["Bool"] = 0, ["Id"] = 0, ["List"] = 1, ["Option"] = 1,
    };

    public static PType ListOf(PType element) => new("List", [element]);
    public static PType OptionOf(PType inner) => new("Option", [inner]);
    public static PType EndpointInput(string endpoint) => new($"{endpoint}.Input", []);
    public static PType EndpointOutput(string endpoint) => new($"{endpoint}.Output", []);

    public bool IsError => Name == "?";
    public bool IsList => Name == "List" && Arguments.Count == 1;
    public bool IsOption => Name == "Option" && Arguments.Count == 1;
    public bool IsEndpointShape => Name.Contains('.');

    public bool Equals(PType? other) => other is not null && ToString() == other.ToString();
    public override int GetHashCode() => ToString().GetHashCode(StringComparison.Ordinal);
    public override string ToString() => Arguments.Count == 0 ? Name : $"{Name}<{string.Join(", ", Arguments)}>";
}
