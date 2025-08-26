using Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos.Filters;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos.Submits;
using HsnSoft.Base.Application.Dtos;

namespace Eds.IdentityService.Application.Contracts.AppUserDomain.Services;

public interface IAppUserAppService
{
    Task<AppUserDto> GetAsync(Guid id);

    Task<PagedResultDto<AppUserDto>> GetPagedListAsync(GetAppUsersPaged pagedInput);
    Task<List<AppUserDto>> GetFilterListAsync(GetAppUsersFilter filterInput);
    Task<List<AppUserDto>> GetSearchListAsync(GetAppUsersSearch searchInput);

    Task<AppUserDto> CreateAsync(AppUserCreateDto input);

    Task UpdateAsync(AppUserUpdateDto input);

    Task DeleteAsync(Guid id);
}