using System.Security.Claims;
using Application.Common.Interfaces.ClaimInterface;
using Application.Common.Security;

namespace Application.Services;

public class ClaimService : IClaimInterface
{
    public ClaimService(IHttpContextAccessor httpContextAccessor)
    {
        var identity = httpContextAccessor.HttpContext?.User?.Identity as ClaimsIdentity;
        var extractedId = AuthenTools.GetCurrentAccountId(identity!);
        var username = AuthenTools.GetCurrentUsername(identity!);
        var fullname = AuthenTools.GetCurrentFullname(identity!);
        var email = AuthenTools.GetCurrentEmail(identity!);
        var role = AuthenTools.GetRole(identity!);
        GetCurrentUserId = string.IsNullOrEmpty(extractedId) ? Guid.Empty : new Guid(extractedId);
        GetCurrentUsername = string.IsNullOrEmpty(username) ? "" : username;
        GetCurrentEmail = string.IsNullOrEmpty(email) ? "" : email;
        GetCurrentFullname = string.IsNullOrEmpty(fullname) ? "" : fullname;
        GetRole = string.IsNullOrEmpty(role) ? "" : role;
    }
    public Guid GetCurrentUserId { get; }
    public string GetCurrentUsername { get; }
    public string GetCurrentEmail { get; }
    public string GetCurrentFullname { get; }
    public string GetRole { get; }
}
