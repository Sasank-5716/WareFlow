using Microsoft.AspNetCore.Identity;

namespace Wareflow.Infrastructure.Identity;

public sealed class WareflowIdentityUser : IdentityUser
{
    public string? DisplayName { get; set; }
}
