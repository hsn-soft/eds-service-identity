using Eds.Shared.Contracts.Cache;
using Eds.Shared.Contracts.Cache.ServicePermissions;

namespace Eds.IdentityService.Application;

public sealed class ApplicationPermissionProvider : IServicePermissionProvider
{
    public Task<List<string>> GetServicePermissionKeysAsync() => Task.FromResult(IdentityServicePermissions.GetAll().ToList());
}