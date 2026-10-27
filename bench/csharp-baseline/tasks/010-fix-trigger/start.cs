namespace Greeting;
public static class Greeter
{
    public static string SayHello() => "goodbye";
}
public sealed class GreeterTests
{
    [Xunit.Fact]
    public void Greeter_returns_hello() => Xunit.Assert.Equal("hello", Greeter.SayHello());
}
