namespace Wareflow.Application.Authentication;

public sealed record RegisterRequest(string Email, string Password, string? DisplayName);

public sealed record LoginRequest(string Email, string Password, bool RememberMe);

public sealed record ForgotPasswordRequest(string Email);

public sealed record ResetPasswordRequest(
    string Email,
    string Token,
    string NewPassword);

public sealed record ChangePasswordRequest(
    string CurrentPassword,
    string NewPassword);

public sealed record ConfirmEmailRequest(string UserId, string Token);

public sealed record AuthenticatedUser(
    string Id,
    string Email,
    string? DisplayName,
    bool IsEmailConfirmed);

public sealed record AuthenticationResult(
    bool Succeeded,
    bool RequiresEmailConfirmation,
    bool IsLockedOut,
    string? ErrorCode = null);

public interface IAuthenticationService
{
    Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        string? origin,
        CancellationToken cancellationToken);

    Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken);

    Task LogoutAsync(CancellationToken cancellationToken);

    Task<AuthenticatedUser?> GetCurrentUserAsync(
        CancellationToken cancellationToken);

    Task<bool> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken);

    Task<AuthenticationResult> RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        string? origin,
        CancellationToken cancellationToken);

    Task<AuthenticationResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken);

    Task<AuthenticationResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task SendAsync(
        string recipient,
        string subject,
        string htmlBody,
        CancellationToken cancellationToken);
}
