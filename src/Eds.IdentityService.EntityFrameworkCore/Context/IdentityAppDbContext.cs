using Eds.IdentityService.Domain.AppRoleDomain.Entities;
using Eds.IdentityService.Domain.AppUserDomain.Entities;
using Eds.IdentityService.EntityFrameworkCore.Configurations;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eds.IdentityService.EntityFrameworkCore.Context;

public sealed class IdentityAppDbContext : IdentityDbContext<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken>
{
    //AspNetRoleClaims
    //AspNetRoles
    //AspNetUserClaims
    //AspNetUserLogins
    //AspNetUserRoles
    //AspNetUserTokens
    //AspNetUsers
    // public DbSet<PermissionGrant> PermissionGrants { get; set; }

    public IdentityAppDbContext(DbContextOptions<IdentityAppDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureIdentityAppEntities();
    }
}