namespace Wareflow.Application.Foundation;

public interface IFoundationProbeValidator
{
    IReadOnlyDictionary<string, string[]> Validate(FoundationProbeRequest request);
}
