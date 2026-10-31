namespace Wareflow.Infrastructure.Identity;

public sealed class IdentityEmailOptions
{
    public string PublicOrigin { get; set; } = "http://localhost:4200";

    public string FromAddress { get; set; } = "no-reply@wareflow.local";

    public string FromName { get; set; } = "WAREFLOW";
}

public sealed class SmtpOptions
{
    public string Host { get; set; } = "localhost";

    public int Port { get; set; } = 1025;

    public string? UserName { get; set; }

    public string? Password { get; set; }

    public bool UseSsl { get; set; }
}
