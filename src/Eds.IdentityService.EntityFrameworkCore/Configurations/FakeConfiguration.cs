using Eds.IdentityService.Domain.FakeDomain.Consts;
using Eds.IdentityService.Domain.FakeDomain.Entities;
using HsnSoft.Base;
using Microsoft.EntityFrameworkCore;

namespace Eds.IdentityService.EntityFrameworkCore.Configurations;

public static class FakeConfiguration
{
    public static void ConfigureFakeEntity(this ModelBuilder builder)
    {
        Check.NotNull(builder, nameof(builder));

        builder.Entity<Fake>(b =>
        {
            b.ToTable(EfCoreDbProperties.DbTablePrefix + FakeConsts.TableName, EfCoreDbProperties.DbSchema);
            b.HasKey(ci => ci.Id);

            b.Property(x => x.FakeDate).HasColumnName(nameof(Fake.FakeDate)).IsRequired();
            b.Property(x => x.FakeCode).HasColumnName(nameof(Fake.FakeCode)).IsRequired().HasMaxLength(FakeConsts.FakeCodeMaxLength);
            b.Property(x => x.FakeState).HasColumnName(nameof(Fake.FakeState)).IsRequired();
        });
    }
}