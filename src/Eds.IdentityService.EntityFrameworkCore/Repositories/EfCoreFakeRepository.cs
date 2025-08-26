using System.Globalization;
using System.Linq.Dynamic.Core;
using Eds.IdentityService.Domain.Enums;
using Eds.IdentityService.Domain.FakeDomain.Consts;
using Eds.IdentityService.Domain.FakeDomain.Entities;
using Eds.IdentityService.Domain.FakeDomain.Exceptions;
using Eds.IdentityService.Domain.FakeDomain.Repositories;
using Eds.IdentityService.EntityFrameworkCore.Context;
using HsnSoft.Base.Domain.Repositories.EntityFrameworkCore;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;

namespace Eds.IdentityService.EntityFrameworkCore.Repositories;

public sealed class EfCoreFakeRepository : EfCoreGenericRepository<IdentityServiceDbContext, Fake, Guid>, IFakeRepository
{
    public EfCoreFakeRepository(IServiceProvider provider, IdentityServiceDbContext dbContext) : base(provider, dbContext)
    {
    }

    public async Task<Fake> CreateAsync(
        DateTime fakeDate,
        string fakeCode,
        FakeState fakeState)
        => await CreateAsync(Guid.NewGuid(),
            fakeDate,
            fakeCode,
            fakeState);

    public async Task<Fake> CreateAsync(
        Guid id,
        DateTime fakeDate,
        string fakeCode,
        FakeState fakeState)
    {
        if (id == Guid.Empty) id = Guid.NewGuid();

        // Create draft Fake
        var draft = new Fake(
            id: id,
            fakeDate: fakeDate,
            fakeCode: fakeCode,
            fakeState: fakeState
        );

        //Domain Rules
        // Rule01
        // Rule02

        return await InsertAsync(draft);
    }

    public async Task<Fake> UpdateAsync(Guid id,
        DateTime fakeDate,
        string fakeCode,
        FakeState fakeState)
    {
        var oldEntity = await FindAsync(id, false);
        if (oldEntity == null)
        {
            throw new FakeNotFoundException(id.ToString());
        }

        oldEntity.SetFakeDate(fakeDate);
        oldEntity.SetFakeCode(fakeCode);
        oldEntity.FakeState = fakeState;

        //Domain Rules
        // Rule01
        // Rule02

        return await UpdateAsync(oldEntity);
    }

    public async Task RemoveAsync(Guid id)
    {
        var entity = await FindAsync(id, false);
        if (entity == null)
        {
            throw new FakeNotFoundException(id.ToString());
        }

        //Domain Rule -> Fake Dependency Control for Delete
        // Rule01

        var guidGenerated = Guid.NewGuid().ToString("N").ToUpper();
        var uniqueField = guidGenerated + "_" + entity.FakeCode;
        if (uniqueField.Length > FakeConsts.FakeCodeMaxLength)
        {
            uniqueField = uniqueField[..FakeConsts.FakeCodeMaxLength];
        }

        entity.SetFakeCode(uniqueField);
        entity.IsDeleted = true;
        await UpdateAsync(entity);
    }

    public async Task<List<Fake>> GetPagedListWithFiltersAsync(
        DateTime? fakeDate = null,
        string fakeCode = null,
        FakeState? fakeState = null,
        string sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = includeDetails
            ? WithDetails(DefaultPropertySelector?.ToArray())
            : GetQueryable();

        var query = ApplyFilter(queryable, null,
            fakeDate, fakeCode, fakeState);

        return await query
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? FakeConsts.GetDefaultSorting() : sorting)
            .PageBy(skipCount, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<long> GetCountWithFiltersAsync(
        DateTime? fakeDate = null,
        string fakeCode = null,
        FakeState? fakeState = null,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyFilter(GetQueryable(), null,
            fakeDate, fakeCode, fakeState);

        return await query.LongCountAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<List<Fake>> GetFilterListAsync(
        DateTime? fakeDate = null,
        string fakeCode = null,
        FakeState? fakeState = null,
        string sorting = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    )
    {
        var queryable = includeDetails
            ? WithDetails(DefaultPropertySelector?.ToArray())
            : GetQueryable();

        var query = ApplyFilter(queryable, null,
            fakeDate, fakeCode, fakeState);

        return await query
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? FakeConsts.GetDefaultSorting() : sorting)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    public async Task<List<Fake>> GetSearchListAsync(
        string searchText = null,
        string sorting = null,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default
    )
    {
        var query = ApplyFilter(GetQueryable(), searchText);

        return await query
            .OrderBy(string.IsNullOrWhiteSpace(sorting) ? FakeConsts.GetDefaultSorting() : sorting)
            .PageBy(0, maxResultCount)
            .ToListAsync(GetCancellationToken(cancellationToken));
    }

    private IQueryable<Fake> ApplyFilter(
        IQueryable<Fake> query,
        [CanBeNull] string searchText = null,
        DateTime? fakeDate = null,
        [CanBeNull] string fakeCode = null,
        FakeState? fakeState = null
    )
    {
        searchText = searchText?.ToLower(new CultureInfo("en-US"));
        fakeCode = fakeCode?.ToLower(new CultureInfo("en-US"));

        if (fakeDate.HasValue)
        {
            var fakeStartTime = fakeDate.Value.Date;
            var fakeEndTime = fakeStartTime.AddDays(1);
            query = query.Where(x => x.FakeDate >= fakeStartTime && x.FakeDate < fakeEndTime);
        }

        return query
            .WhereIf(!string.IsNullOrWhiteSpace(searchText), e => e.FakeCode.ToLower(new CultureInfo("en-US")).Contains(searchText))
            .WhereIf(!string.IsNullOrWhiteSpace(fakeCode), e => e.FakeCode.ToLower(new CultureInfo("en-US")).Contains(fakeCode))
            .WhereIf(fakeState.HasValue, e => e.FakeState == fakeState.Value);
    }
}