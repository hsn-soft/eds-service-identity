using HsnSoft.Base.Application.Dtos;
using JetBrains.Annotations;

namespace Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos.Filters;

public sealed class GetAppUsersFilter : SortedResultRequestDto
{
    public Guid? TenantId { get; set; } = null;

    // Default identity model
    [CanBeNull]
    public string UserName { get; set; } = null;

    [CanBeNull]
    public string Email { get; set; } = null;

    public bool? EmailConfirmed { get; set; } = null;

    [CanBeNull]
    public string PhoneNumber { get; set; } = null;

    public bool? PhoneNumberConfirmed { get; set; } = null;

    // Custom identity model
    [CanBeNull]
    public string Name { get; set; } = null;

    [CanBeNull]
    public string Surname { get; set; } = null;
}