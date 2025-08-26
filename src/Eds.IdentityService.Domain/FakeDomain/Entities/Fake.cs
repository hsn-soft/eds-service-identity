using Eds.IdentityService.Domain.Enums;
using Eds.IdentityService.Domain.FakeDomain.Consts;
using HsnSoft.Base;
using HsnSoft.Base.Domain.Entities.Auditing;
using JetBrains.Annotations;

namespace Eds.IdentityService.Domain.FakeDomain.Entities;

public sealed class Fake : FullAuditedAggregateRoot<Guid>
{
    public DateTime FakeDate { get; private set; }

    [NotNull]
    public string FakeCode { get; private set; }

    public FakeState FakeState { get; internal set; }

    private Fake()
    {
        FakeCode = string.Empty;
    }

    internal Fake(Guid id, DateTime fakeDate, string fakeCode, FakeState fakeState) : this()
    {
        Id = id;
        SetFakeDate(fakeDate);
        SetFakeCode(fakeCode);
        FakeState = fakeState;
    }

    internal void SetFakeDate(DateTime fakeDate)
    {
        if (fakeDate > DateTime.Now)
        {
            throw new ArgumentException("FakeDate is invalid", nameof(fakeDate));
        }

        FakeDate = fakeDate.ToUniversalTime().Date;
    }

    internal void SetFakeCode(string fakeCode)
    {
        FakeCode = Check.NotNull(fakeCode, nameof(fakeCode), FakeConsts.FakeCodeMaxLength);
    }
}