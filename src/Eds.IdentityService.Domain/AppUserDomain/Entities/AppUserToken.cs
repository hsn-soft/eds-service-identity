using Microsoft.AspNetCore.Identity;

namespace Eds.IdentityService.Domain.AppUserDomain.Entities;

public sealed class AppUserToken : IdentityUserToken<Guid>
{
}