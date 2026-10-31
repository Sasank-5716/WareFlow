using Microsoft.AspNetCore.Antiforgery;
using Wareflow.Application.Authentication;

namespace Wareflow.Api.Endpoints;

public static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Authentication");

        group.MapGet("/csrf", (IAntiforgery antiforgery, HttpContext context) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(context);
            return Results.NoContent();
        });

        group.MapPost("/register", async (
            RegisterRequest request,
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            var result = await authentication.RegisterAsync(
                request,
                null,
                cancellationToken);
            return result.Succeeded
                ? Results.StatusCode(StatusCodes.Status201Created)
                : Results.BadRequest(new { error = result.ErrorCode });
        })
        .RequireRateLimiting("auth")
        .RequireAntiforgery();

        group.MapPost("/login", async (
            LoginRequest request,
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            var result = await authentication.LoginAsync(request, cancellationToken);
            if (result.Succeeded)
            {
                return Results.NoContent();
            }

            return result.IsLockedOut
                ? Results.StatusCode(StatusCodes.Status423Locked)
                : Results.Unauthorized();
        })
        .RequireRateLimiting("auth")
        .RequireAntiforgery();

        group.MapPost("/logout", async (
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            await authentication.LogoutAsync(cancellationToken);
            return Results.NoContent();
        })
        .RequireAuthorization()
        .RequireAntiforgery();

        group.MapGet("/me", async (
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            var user = await authentication.GetCurrentUserAsync(cancellationToken);
            return user is null ? Results.Unauthorized() : Results.Ok(user);
        })
        .RequireAuthorization();

        group.MapPost("/confirm-email", async (
            ConfirmEmailRequest request,
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            var confirmed = await authentication.ConfirmEmailAsync(request, cancellationToken);
            return confirmed ? Results.NoContent() : Results.BadRequest();
        })
        .RequireAntiforgery();

        group.MapPost("/forgot-password", async (
            ForgotPasswordRequest request,
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            var result = await authentication.RequestPasswordResetAsync(
                request,
                null,
                cancellationToken);
            return result.ErrorCode == "email_delivery_failed"
                ? Results.StatusCode(StatusCodes.Status503ServiceUnavailable)
                : Results.Accepted();
        })
        .RequireRateLimiting("auth")
        .RequireAntiforgery();

        group.MapPost("/reset-password", async (
            ResetPasswordRequest request,
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            var result = await authentication.ResetPasswordAsync(request, cancellationToken);
            return result.Succeeded
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.ErrorCode });
        })
        .RequireRateLimiting("auth")
        .RequireAntiforgery();

        group.MapPost("/change-password", async (
            ChangePasswordRequest request,
            IAuthenticationService authentication,
            CancellationToken cancellationToken) =>
        {
            var result = await authentication.ChangePasswordAsync(request, cancellationToken);
            return result.Succeeded
                ? Results.NoContent()
                : Results.BadRequest(new { error = result.ErrorCode });
        })
        .RequireAuthorization()
        .RequireAntiforgery();

        return endpoints;
    }
}

internal static class AntiforgeryEndpointExtensions
{
    public static RouteHandlerBuilder RequireAntiforgery(
        this RouteHandlerBuilder builder) => builder;
}
