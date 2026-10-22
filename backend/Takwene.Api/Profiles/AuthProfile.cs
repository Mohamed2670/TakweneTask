using AutoMapper;
using Takwene.Api.DTOs.Auth;
using Takwene.Api.Models;

namespace Takwene.Api.Profiles;

public sealed class AuthProfile : Profile
{
    public AuthProfile()
    {
        CreateMap<User, UserResponse>();
    }
}
