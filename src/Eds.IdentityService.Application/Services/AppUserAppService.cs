using System.Net;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos.Filters;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos.Submits;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Services;
using Eds.IdentityService.Domain.AppUserDomain.Entities;
using Eds.IdentityService.Domain.AppUserDomain.Repositories;
using HsnSoft.Base;
using HsnSoft.Base.Application.Dtos;
using HsnSoft.Base.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Eds.IdentityService.Application.Services;

public sealed class AppUserAppService : ApplicationServiceBase, IAppUserAppService
{
    private readonly IBaseLogger _logger;
    private readonly IAppUserRepository _appUserRepository;

    public AppUserAppService(IServiceProvider provider,
        IAppUserRepository appUserRepository
    ) : base(provider)
    {
        _logger = provider.GetRequiredService<IBaseLogger>();

        _appUserRepository = appUserRepository;
    }

    public async Task<AppUserDto> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var item = await _appUserRepository.FindWithIdAsync(id);
        if (item == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.NotFound);
        }

        var resultItem = Mapper.Map<AppUser, AppUserDto>(item);
        resultItem.Roles = await _appUserRepository.GetUserRolesAsync(item);
        resultItem.Roles ??= new List<string>();

        return resultItem;
    }

    public async Task<PagedResultDto<AppUserDto>> GetPagedListAsync(GetAppUsersPaged pagedInput)
    {
        if (pagedInput == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var roleIds = pagedInput.Roles is { Count: > 0 } ? pagedInput.Roles.Select(x => x.RoleId).ToList() : null;

        long totalCount = await _appUserRepository.GetCountWithFiltersAsync(pagedInput.TenantId,
            pagedInput.UserName, pagedInput.Email, pagedInput.EmailConfirmed, pagedInput.PhoneNumber, pagedInput.PhoneNumberConfirmed,
            pagedInput.Name, pagedInput.Surname, roleIds);

        var items = await _appUserRepository.GetPagedListWithFiltersAsync(pagedInput.TenantId,
            pagedInput.UserName, pagedInput.Email, pagedInput.EmailConfirmed, pagedInput.PhoneNumber, pagedInput.PhoneNumberConfirmed,
            pagedInput.Name, pagedInput.Surname, roleIds,
            pagedInput.SortingText, pagedInput.MaxResultCount, pagedInput.ResultPageNumber);

        if (items == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.RequestTimeout);
        }

        var resultItems = new List<AppUserDto>();
        foreach (var item in items)
        {
            var resultItem = Mapper.Map<AppUser, AppUserDto>(item);
            resultItem.Roles = await _appUserRepository.GetUserRolesAsync(item);
            resultItem.Roles ??= new List<string>();

            resultItem.UserName = (!string.IsNullOrWhiteSpace(resultItem.UserName) && resultItem.UserName.Contains('#'))
                ? resultItem.UserName.Split("#")[0]
                : resultItem.UserName;

            resultItem.Email = (!string.IsNullOrWhiteSpace(resultItem.Email) && resultItem.Email.Contains('#'))
                ? resultItem.Email.Split("#")[0]
                : resultItem.Email;

            if (string.IsNullOrWhiteSpace(resultItem.AvatarSuffixUrl)) { resultItem.AvatarSuffixUrl = "/images/no-image.webp"; }

            resultItems.Add(resultItem);
        }

        return new PagedResultDto<AppUserDto> { TotalCount = totalCount, Items = resultItems };
    }

    public async Task<List<AppUserDto>> GetFilterListAsync(GetAppUsersFilter filterInput)
    {
        if (filterInput == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var items = await _appUserRepository.GetFilterListAsync(filterInput.TenantId,
            filterInput.UserName, filterInput.Email, filterInput.EmailConfirmed, filterInput.PhoneNumber, filterInput.PhoneNumberConfirmed,
            filterInput.Name, filterInput.Surname, null,
            filterInput.SortingText);

        if (items == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.RequestTimeout);
        }

        foreach (var item in items)
        {
            var result = Mapper.Map<AppUser, AppUserDto>(item);
            result.Roles = await _appUserRepository.GetUserRolesAsync(item);
            result.Roles ??= new List<string>();
        }

        var resultItems = new List<AppUserDto>();
        foreach (var item in items)
        {
            var resultItem = Mapper.Map<AppUser, AppUserDto>(item);
            resultItem.Roles = await _appUserRepository.GetUserRolesAsync(item);
            resultItem.Roles ??= new List<string>();

            resultItems.Add(resultItem);
        }

        return resultItems;
    }

    public async Task<List<AppUserDto>> GetSearchListAsync(GetAppUsersSearch searchInput)
    {
        if (searchInput == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var items = await _appUserRepository.GetSearchListAsync(searchInput.TenantId,
            searchInput.SearchText, searchInput.SortingText, searchInput.MaxResultCount);

        if (items == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.RequestTimeout);
        }

        var resultItems = new List<AppUserDto>();
        foreach (var item in items)
        {
            var resultItem = Mapper.Map<AppUser, AppUserDto>(item);
            resultItem.Roles = await _appUserRepository.GetUserRolesAsync(item);
            resultItem.Roles ??= new List<string>();

            resultItems.Add(resultItem);
        }

        return resultItems;
    }

    public async Task<AppUserDto> CreateAsync(AppUserCreateDto input)
    {
        if (input == null)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        var appUser = await _appUserRepository.CreateAsync(
            tenantId: input.TenantId ?? Guid.Empty,
            tenantDomain: input.TenantDomain,
            userName: input.UserName,
            email: input.Email,
            phone: input.PhoneNumber,
            name: input.Name,
            surname: input.Surname,
            defaultLanguage: input.DefaultLanguage,
            avatarSuffixUrl: input.AvatarSuffixUrl,
            plainPassword: "Passw0rd!",
            roles: input.Roles is { Count: > 0 } ? input.Roles : new List<string>()
        );

        //INTEGRATION EVENT TRIGGER
        //
        //

        return Mapper.Map<AppUser, AppUserDto>(appUser);
    }

    public async Task UpdateAsync(AppUserUpdateDto input)
    {
        if (input == null || input.Id == Guid.Empty)
        {
            throw new BaseHttpException((int)HttpStatusCode.BadRequest);
        }

        await _appUserRepository.UpdateAsync(
            id: input.Id,
            userName: input.UserName,
            email: input.Email,
            phone: input.PhoneNumber,
            name: input.Name,
            surname: input.Surname,
            defaultLanguage: input.DefaultLanguage,
            roles: input.Roles is { Count: > 0 } ? input.Roles : new List<string>()
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

        await _appUserRepository.DeleteAsync(id);

        //INTEGRATION EVENT TRIGGER
        //
        //
    }
}