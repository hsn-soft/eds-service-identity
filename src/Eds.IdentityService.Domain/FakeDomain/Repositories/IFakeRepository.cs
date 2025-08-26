using Eds.IdentityService.Domain.Enums;
using Eds.IdentityService.Domain.FakeDomain.Entities;
using HsnSoft.Base.Domain.Repositories;
using JetBrains.Annotations;

namespace Eds.IdentityService.Domain.FakeDomain.Repositories;

public interface IFakeRepository : IReadOnlyGenericRepository<Fake, Guid>
{
    Task<Fake> CreateAsync(DateTime fakeDate, [NotNull] string fakeCode, FakeState fakeState);

    Task<Fake> CreateAsync(Guid id, DateTime fakeDate, [NotNull] string fakeCode, FakeState fakeState);

    Task<Fake> UpdateAsync(Guid id, DateTime fakeDate, [NotNull] string fakeCode, FakeState fakeState);

    Task RemoveAsync(Guid id);

    Task<List<Fake>> GetPagedListWithFiltersAsync(
        DateTime? fakeDate = null,
        [CanBeNull] string fakeCode = null,
        FakeState? fakeState = null,
        string sorting = null,
        int maxResultCount = int.MaxValue,
        int skipCount = 0,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    );

    Task<long> GetCountWithFiltersAsync(
        DateTime? fakeDate = null,
        [CanBeNull] string fakeCode = null,
        FakeState? fakeState = null,
        CancellationToken cancellationToken = default
    );

    Task<List<Fake>> GetFilterListAsync(
        DateTime? fakeDate = null,
        [CanBeNull] string fakeCode = null,
        FakeState? fakeState = null,
        string sorting = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default
    );

    Task<List<Fake>> GetSearchListAsync(
        string searchText = null,
        string sorting = null,
        int maxResultCount = int.MaxValue,
        CancellationToken cancellationToken = default
    );
}