using HsnSoft.Base.Application.Dtos;

namespace Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos.Filters;

public sealed class GetAppUsersSearch : SearchAndSortedResultRequestDto
{
    public Guid? TenantId { get; set; } = null;
}