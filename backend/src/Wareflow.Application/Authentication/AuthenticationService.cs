namespace Wareflow.Application.Authentication;

public sealed class AuthenticationService(
    IAuthenticationServiceProvider provider) : IAuthenticationService
{
    public Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        string? origin,
        CancellationToken cancellationToken) =>
        provider.RegisterAsync(request, origin, cancellationToken);

    public Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken) =>
        provider.LoginAsync(request, cancellationToken);

    public Task LogoutAsync(CancellationToken cancellationToken) =>
        provider.LogoutAsync(cancellationToken);

    public Task<AuthenticatedUser?> GetCurrentUserAsync(
        CancellationToken cancellationToken) =>
        provider.GetCurrentUserAsync(cancellationToken);

    public Task<bool> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken) =>
        provider.ConfirmEmailAsync(request, cancellationToken);

    public Task<AuthenticationResult> RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        string? origin,
        CancellationToken cancellationToken) =>
        provider.RequestPasswordResetAsync(request, origin, cancellationToken);

    public Task<AuthenticationResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken) =>
        provider.ResetPasswordAsync(request, cancellationToken);

    public Task<AuthenticationResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken) =>
        provider.ChangePasswordAsync(request, cancellationToken);
}

public interface IAuthenticationServiceProvider : IAuthenticationService
{
}
