using Wareflow.Application.Foundation;

namespace Wareflow.UnitTests;

public sealed class FoundationProbeValidatorTests
{
    [Fact]
    public void Empty_message_is_invalid()
    {
        var validator = new FoundationProbeValidator();

        var errors = validator.Validate(new FoundationProbeRequest(" "));

        Assert.True(errors.ContainsKey(nameof(FoundationProbeRequest.Message)));
    }

    [Fact]
    public void Message_within_limit_is_valid()
    {
        var validator = new FoundationProbeValidator();

        var errors = validator.Validate(new FoundationProbeRequest("ready"));

        Assert.Empty(errors);
    }
}
