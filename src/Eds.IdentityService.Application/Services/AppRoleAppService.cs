using System.Net;
using Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos;
using Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos.Filters;
using Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos.Submits;
using Eds.IdentityService.Application.Contracts.AppRoleDomain.Services;
using Eds.IdentityService.Domain.AppRoleDomain.Entities;
using Eds.IdentityService.Domain.AppRoleDomain.Repositories;
using HsnSoft.Base;
using HsnSoft.Base.Application.Dtos;
using HsnSoft.Base.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Eds.IdentityService.Application.Services;

public sealed class AppRoleAppService : ApplicationServiceBase, IAppRoleAppService
{
    private readonly IBaseLogger _logger;
    private readonly IAppRoleRepository _appRoleRepository;

    public AppRoleAppService(IServiceProvider provider,
        IAppRoleRepository appRoleRepository
    ) : base(provider)
    {
        _logger = provider.GetRequiredService<IBaseLogger>();

        _appRoleRepository = appRoleRepository;
    }

    public async Task<AppRoleDto> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var item = await _appRoleRepository.FindWithIdAsync(id);
        if (item == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.NotFound);
        }

        return Mapper.Map<AppRole, AppRoleDto>(item);
    }

    public async Task<PagedResultDto<AppRoleDto>> GetPagedListAsync(GetAppRolesPaged pagedInput)
    {
        if (pagedInput == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var totalCount = await _appRoleRepository.GetCountWithFiltersAsync(pagedInput.TenantId,
            pagedInput.Name, pagedInput.IsDefault, pagedInput.IsStatic, pagedInput.IsPublic);

        var items = await _appRoleRepository.GetPagedListWithFiltersAsync(pagedInput.TenantId,
            pagedInput.Name, pagedInput.IsDefault, pagedInput.IsStatic, pagedInput.IsPublic,
            pagedInput.Sorting, pagedInput.MaxResultCount, pagedInput.SkipCount);

        if (items == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.RequestTimeout);
        }

        return new PagedResultDto<AppRoleDto> { TotalCount = totalCount, Items = Mapper.Map<List<AppRole>, List<AppRoleDto>>(items) };
    }

    public async Task<List<AppRoleDto>> GetFilterListAsync(GetAppRolesFilter filterInput)
    {
        if (filterInput == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var items = await _appRoleRepository.GetFilterListAsync(filterInput.TenantId,
            filterInput.Name, filterInput.IsDefault, filterInput.IsStatic, filterInput.IsPublic,
            filterInput.Sorting);

        if (items == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.RequestTimeout);
        }

        return Mapper.Map<List<AppRole>, List<AppRoleDto>>(items);
    }

    public async Task<List<AppRoleDto>> GetSearchListAsync(GetAppRolesSearch searchInput)
    {
        if (searchInput == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var items = await _appRoleRepository.GetSearchListAsync(searchInput.TenantId,
            searchInput.SearchText, searchInput.Sorting, searchInput.MaxResultCount);

        if (items == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.RequestTimeout);
        }

        return Mapper.Map<List<AppRole>, List<AppRoleDto>>(items);
    }

    public async Task<AppRoleDto> CreateAsync(AppRoleCreateDto input)
    {
        if (input == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var appRole = await _appRoleRepository.CreateAsync(
            tenantId: input.TenantId ?? Guid.Empty,
            tenantDomain: input.TenantDomain,
            name: input.Name ?? string.Empty,
            isDefault: input.IsDefault,
            isStatic: input.IsStatic,
            isPublic: input.IsPublic
        );

        //INTEGRATION EVENT TRIGGER
        //
        //

        return Mapper.Map<AppRole, AppRoleDto>(appRole);
    }

    public async Task UpdateAsync(AppRoleUpdateDto input)
    {
        if (input == null || input.Id == Guid.Empty)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        await _appRoleRepository.UpdateAsync(
            id: input.Id,
            name: input.Name ?? string.Empty,
            isDefault: input.IsDefault,
            isStatic: input.IsStatic,
            isPublic: input.IsPublic
        );

        //INTEGRATION EVENT TRIGGER
        //
        //
    }

    public async Task DeleteAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        await _appRoleRepository.DeleteAsync(id);

        //INTEGRATION EVENT TRIGGER
        // TODO: Create ROLE_DELETED event for AdminService PermissionGrant synch service !!!
        //
    }
}