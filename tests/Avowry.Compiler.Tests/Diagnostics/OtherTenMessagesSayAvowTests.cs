using Avowry.Compiler.Semantics;

namespace Avowry.Compiler.Tests.Diagnostics;

// Feature: The other diagnostic messages name the avow (card 324 criterion 3)
public class OtherTenMessagesSayAvowTests
{
    [Theory]
    [Trait("Criterion", "3")]
    [InlineData("M09-PH325")]
    [InlineData("M10-PH328")]
    [InlineData("M11-PH330")]
    [InlineData("M12-PH333")]
    [InlineData("M13-PH001")]
    [InlineData("M14a-PH208-native-then-steps")]
    [InlineData("M14b-PH208-steps-then-native")]
    [InlineData("M15-PH200-template")]
    [InlineData("M16-PH200-clause")]
    [InlineData("M17-PH200-call")]
    [InlineData("M18-PH200-trigger")]
    public void The_other_ten_messages_say_Avow(string id)
    {
        // Given a remaining trigger source; when compiled; then each baseline row names the avow
        var diagnostics = Resolver.Resolve(TriggerSources.Source(id)).Diagnostics;
        foreach (var row in TriggerSources.Baseline(id))
        {
            var actual = diagnostics.Single(d => d.Code == row.Code && d.Line == row.Line && d.Column == row.Column);
            Assert.Equal(TriggerSources.ToNew(row.Text), actual.Message);
        }
    }
}
