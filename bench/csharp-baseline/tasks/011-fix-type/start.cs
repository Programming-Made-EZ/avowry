namespace Count;
public static class Counter
{
    public static int Answer() => 41;
}
public sealed class CounterTests
{
    [Xunit.Fact]
    public void Counter_returns_forty_two() => Xunit.Assert.Equal(42, Counter.Answer());
}
