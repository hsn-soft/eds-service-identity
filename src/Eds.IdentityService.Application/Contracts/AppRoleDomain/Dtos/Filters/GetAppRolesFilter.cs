using HsnSoft.Base.Application.Dtos;
using JetBrains.Annotations;

namespace Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos.Filters;

public sealed class GetAppRolesFilter : SortedResultRequestDto
{
    public Guid? TenantId { get; set; } = null;

    [CanBeNull]
    public string Name { get; set; } = null;

    public bool? IsDefault { get; set; } = null;
    public bool? IsStatic { get; set; } = null;
    public bool? IsPublic { get; set; } = null;
}