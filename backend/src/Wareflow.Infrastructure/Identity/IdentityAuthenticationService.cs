using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Wareflow.Application.Authentication;

namespace Wareflow.Infrastructure.Identity;

public sealed class IdentityAuthenticationService(
    UserManager<WareflowIdentityUser> userManager,
    SignInManager<WareflowIdentityUser> signInManager,
    IEmailSender emailSender,
    IOptions<IdentityEmailOptions> emailOptions,
    ILogger<IdentityAuthenticationService> logger)
    : IAuthenticationServiceProvider
{
    public async Task<AuthenticationResult> RegisterAsync(
        RegisterRequest request,
        string? origin,
        CancellationToken cancellationToken)
    {
        var user = new WareflowIdentityUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? null
                : request.DisplayName.Trim()
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            return new AuthenticationResult(
                false,
                false,
                false,
                result.Errors.FirstOrDefault()?.Code ?? "registration_failed");
        }

        var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
        var confirmationUrl = BuildUrl(
            origin,
            "/confirm-email",
            new Dictionary<string, string?>
            {
                ["userId"] = user.Id,
                ["token"] = Encode(token)
            });

        try
        {
            await emailSender.SendAsync(
                user.Email!,
                "Confirm your WAREFLOW email",
                $"<p>Confirm your email by opening <a href=\"{HtmlEncoder.Default.Encode(confirmationUrl)}\">this link</a>.</p>",
                cancellationToken);
        }
        catch
        {
            return new AuthenticationResult(false, true, false, "email_delivery_failed");
        }

        logger.LogInformation("User registration completed for {UserId}", user.Id);
        return new AuthenticationResult(true, true, false);
    }

    public async Task<AuthenticationResult> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return new AuthenticationResult(false, false, false, "invalid_credentials");
        }

        if (!await userManager.IsEmailConfirmedAsync(user))
        {
            return new AuthenticationResult(false, true, false, "email_not_confirmed");
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            request.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            logger.LogWarning("User account locked after failed authentication for {UserId}", user.Id);
            return new AuthenticationResult(false, false, true, "account_locked");
        }

        return result.Succeeded
            ? new AuthenticationResult(true, false, false)
            : new AuthenticationResult(false, false, false, "invalid_credentials");
    }

    public async Task LogoutAsync(CancellationToken cancellationToken)
    {
        await signInManager.SignOutAsync();
        logger.LogInformation("User logged out.");
    }

    public async Task<AuthenticatedUser?> GetCurrentUserAsync(
        CancellationToken cancellationToken)
    {
        var user = await signInManager.UserManager.GetUserAsync(
            signInManager.Context.User);

        return user is null
            ? null
            : new AuthenticatedUser(
                user.Id,
                user.Email!,
                user.DisplayName,
                user.EmailConfirmed);
    }

    public async Task<bool> ConfirmEmailAsync(
        ConfirmEmailRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(request.UserId);
        if (user is null)
        {
            return false;
        }

        var result = await userManager.ConfirmEmailAsync(user, Decode(request.Token));
        return result.Succeeded;
    }

    public async Task<AuthenticationResult> RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        string? origin,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !await userManager.IsEmailConfirmedAsync(user))
        {
            return new AuthenticationResult(true, false, false);
        }

        var token = await userManager.GeneratePasswordResetTokenAsync(user);
        var resetUrl = BuildUrl(
            origin,
            "/reset-password",
            new Dictionary<string, string?>
            {
                ["email"] = user.Email,
                ["token"] = Encode(token)
            });

        try
        {
            await emailSender.SendAsync(
                user.Email!,
                "Reset your WAREFLOW password",
                $"<p>Reset your password by opening <a href=\"{HtmlEncoder.Default.Encode(resetUrl)}\">this link</a>.</p>",
                cancellationToken);
        }
        catch
        {
            return new AuthenticationResult(false, false, false, "email_delivery_failed");
        }

        return new AuthenticationResult(true, false, false);
    }

    public async Task<AuthenticationResult> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return new AuthenticationResult(false, false, false, "invalid_reset_request");
        }

        var result = await userManager.ResetPasswordAsync(
            user,
            Decode(request.Token),
            request.NewPassword);

        return result.Succeeded
            ? new AuthenticationResult(true, false, false)
            : new AuthenticationResult(false, false, false, "invalid_reset_request");
    }

    public async Task<AuthenticationResult> ChangePasswordAsync(
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        var user = await signInManager.UserManager.GetUserAsync(
            signInManager.Context.User);
        if (user is null)
        {
            return new AuthenticationResult(false, false, false, "unauthenticated");
        }

        var result = await userManager.ChangePasswordAsync(
            user,
            request.CurrentPassword,
            request.NewPassword);

        return result.Succeeded
            ? new AuthenticationResult(true, false, false)
            : new AuthenticationResult(false, false, false, "password_change_failed");
    }

    private string BuildUrl(
        string? origin,
        string path,
        Dictionary<string, string?> query)
    {
        var baseOrigin = string.IsNullOrWhiteSpace(origin)
            ? emailOptions.Value.PublicOrigin
            : origin.TrimEnd('/');
        var queryString = string.Join(
            "&",
            query.Select(pair =>
                $"{Uri.EscapeDataString(pair.Key)}={Uri.EscapeDataString(pair.Value ?? string.Empty)}"));
        return $"{baseOrigin}{path}?{queryString}";
    }

    private static string Encode(string token) =>
        Convert.ToBase64String(Encoding.UTF8.GetBytes(token))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

    private static string Decode(string token) =>
        Encoding.UTF8.GetString(Convert.FromBase64String(
            token.Replace('-', '+').Replace('_', '/') + new string(
                '=',
                (4 - token.Length % 4) % 4)));
}
