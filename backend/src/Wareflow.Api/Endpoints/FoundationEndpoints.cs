using Wareflow.Application.Foundation;

namespace Wareflow.Api.Endpoints;

public static class FoundationEndpoints
{
    public static IEndpointRouteBuilder MapFoundationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/foundation/validate",
            (
                FoundationProbeRequest request,
                IFoundationProbeValidator validator) =>
            {
                var errors = validator.Validate(request);

                if (errors.Count > 0)
                {
                    return Results.ValidationProblem(errors);
                }

                return Results.Ok(new
                {
                    request.Message,
                    UtcTimestamp = DateTimeOffset.UtcNow,
                    DependencyInjection = "available"
                });
            })
            .WithName("ValidateFoundationRequest")
            .WithTags("Foundation");

        return endpoints;
    }
}
