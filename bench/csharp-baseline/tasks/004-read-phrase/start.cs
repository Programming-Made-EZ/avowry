namespace Identity;
public static class Identity {
    public static string Of(string value) => value;
}
public sealed class Spec {
    [Xunit.Fact]
    public void returns_its_input() => Xunit.Assert.Equal("sample", Identity.Of("sample"));
}
