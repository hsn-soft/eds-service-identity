using Eds.IdentityService.EntityFrameworkCore.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Eds.IdentityService.EntityFrameworkCore.Factory;

internal sealed class IdentityAppDbContextFactory : IDesignTimeDbContextFactory<IdentityAppDbContext>
{
    public IdentityAppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<IdentityAppDbContext>()
            .UseNpgsql(DbContextFactoryHelper.GetConnectionStringFromConfiguration(), b =>
            {
                b.MigrationsHistoryTable("__EFMigrationsHistory");
                b.MigrationsAssembly(typeof(IdentityAppDbContext).Assembly.GetName().Name);
            });

        return new IdentityAppDbContext(builder.Options);
    }
}