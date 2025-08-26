using AutoMapper;
using Eds.IdentityService.Application.Contracts.AppRoleDomain.Dtos;
using Eds.IdentityService.Application.Contracts.AppUserDomain.Dtos;
using Eds.IdentityService.Domain.AppRoleDomain.Entities;
using Eds.IdentityService.Domain.AppUserDomain.Entities;
using Eds.Shared.Helper.Utils;

namespace Eds.IdentityService.Application;

public class ApplicationAutoMapperProfile : Profile
{
    public ApplicationAutoMapperProfile()
    {
        CreateMap<AppUser, AppUserDto>()
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(source => StringOperations.SplitFirstValue(source.UserName, "#")))
            .ForMember(dest => dest.Email,
                opt => opt.MapFrom(source => StringOperations.SplitFirstValue(source.Email, "#")));

        CreateMap<AppRole, AppRoleDto>()
            .ForMember(dest => dest.Name,
                opt => opt.MapFrom(source => StringOperations.SplitFirstValue(source.Name, "#")));
    }
}