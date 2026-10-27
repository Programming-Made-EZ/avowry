namespace Names;
public static class Names {
    public static void Ensure(string name) {
        if (string.IsNullOrEmpty(name)) throw new System.Exception("MissingName");
    }
}
public sealed class Spec {
    [Xunit.Fact]
    public void accepts_a_name() => Names.Ensure("Ada");
}
