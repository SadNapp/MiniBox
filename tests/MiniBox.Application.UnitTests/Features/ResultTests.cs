namespace MiniBox.Application.UnitTests.Features;

public class ResultTests
{
    [Test]
    public void Failure_ContainsErrorAndNoValue()
    {
        var result = MiniBox.Domain.Common.Result<string>.Failure("invalid");

        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("invalid"));
    }

    [Test]
    public void Success_ContainsValue()
    {
        var result = MiniBox.Domain.Common.Result<string>.Success("created");

        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.Value, Is.EqualTo("created"));
    }
}
