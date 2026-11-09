namespace Avowry.Adapters.CSharp;

/// <summary>
/// Names the generated C# declares itself. The emitter spells every one of them through these constants,
/// and the reserved-name guard compares user names against the same constants, so the two cannot drift.
/// </summary>
internal static class ReservedNames
{
    public const string AvowAttribute = "AvowAttribute";
    public const string AvowryFailure = "AvowryFailure";
    public const string Avows = "Avows";
    public const string AvowSpecs = "AvowSpecs";
    public const string Program = "Program";

    // File stems of the generated Api project. Endpoint files carry a double extension, so these
    // are file names only, not reserved names for endpoints or types.
    public const string RuntimeFile = "AvowryRuntime";
    public const string TypesFile = "Types";
    public const string BuildFile = "AvowryBuild";

    /// <summary>Top-level types the generated code declares in the module namespace, with what each is for.</summary>
    public static readonly IReadOnlyDictionary<string, string> TypeNames = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [AvowAttribute] = "the generated attribute that links code back to an avow",
        [AvowryFailure] = "the generated exception for declared failures",
        [Avows] = "the generated class that holds every avow",
        [AvowSpecs] = "the generated class that holds the avow specs",
    };

    /// <summary>Avow method names that collide with the enclosing generated class or its failure helper.</summary>
    public static readonly IReadOnlyDictionary<string, string> MethodNames = new Dictionary<string, string>(StringComparer.Ordinal)
    {
        [Avows] = "the generated class that holds every avow",
        [AvowryFailure] = "the generated exception for declared failures",
    };
}
