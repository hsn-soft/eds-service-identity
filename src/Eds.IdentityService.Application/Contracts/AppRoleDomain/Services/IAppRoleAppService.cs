using Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos;
using Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos.Filters;
using Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos.Submits;
using HsnSoft.Base.Application.Dtos;

namespace Eds.IdentityService.Application.Contracts.AppRoleDomain.Services;

public interface IAppRoleAppService
{
    Task<AppRoleDto> GetAsync(Guid id);

    Task<PagedResultDto<AppRoleDto>> GetPagedListAsync(GetAppRolesPaged pagedInput);
    Task<List<AppRoleDto>> GetFilterListAsync(GetAppRolesFilter filterInput);
    Task<List<AppRoleDto>> GetSearchListAsync(GetAppRolesSearch searchInput);

    Task<AppRoleDto> CreateAsync(AppRoleCreateDto input);

    Task UpdateAsync(AppRoleUpdateDto input);

    Task DeleteAsync(Guid id);
}