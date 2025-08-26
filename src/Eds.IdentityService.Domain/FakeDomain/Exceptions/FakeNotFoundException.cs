using HsnSoft.Base;
using HsnSoft.Base.Validation.Localization;

namespace Eds.IdentityService.Domain.FakeDomain.Exceptions;

[Serializable]
internal sealed class FakeNotFoundException : BusinessException
{
    public FakeNotFoundException(string referenceCode)
        : base(errorMessage: DomainErrorCodes.FakeNotFound)
    {
        ErrorCode = DomainErrorCodes.FakeNotFound.Split(':').LastOrDefault();
        WithData(ValidationResourceKeys.ErrorReference, referenceCode);
    }
}