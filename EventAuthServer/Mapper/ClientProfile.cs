using AutoMapper;

namespace EventAuthServer.Mapper
{
    public class ClientProfile : Profile
    {
        public ClientProfile()
        {
            CreateMap<IdentityServer4.EntityFramework.Entities.Client, IdentityServer4.Models.Client>().ReverseMap();
        }
    }
}
