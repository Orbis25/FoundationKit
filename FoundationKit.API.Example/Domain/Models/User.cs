using Microsoft.AspNetCore.Identity;

namespace FoundationKit.API.Example.Domain.Models;

public class User : IdentityUser
{
    public string? FullName { get; set; }
}
