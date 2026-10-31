namespace Wareflow.Application.Foundation;

public sealed class FoundationProbeValidator : IFoundationProbeValidator
{
    public IReadOnlyDictionary<string, string[]> Validate(FoundationProbeRequest request)
    {
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(request.Message))
        {
            errors[nameof(request.Message)] = ["Message is required."];
        }
        else if (request.Message.Length > 100)
        {
            errors[nameof(request.Message)] = ["Message must be 100 characters or fewer."];
        }

        return errors;
    }
}
