using HsnSoft.Base.Application.Dtos;

namespace Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos.Filters;

public sealed class GetAppRolesSearch : SearchAndSortedResultRequestDto
{
    public Guid? TenantId { get; set; } = null;
}