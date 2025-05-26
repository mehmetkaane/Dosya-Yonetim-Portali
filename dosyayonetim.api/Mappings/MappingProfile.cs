using AutoMapper;
using dosyayonetim.api.Models;
using dosyayonetim.api.Models.DTOs;
using dosyayonetim.api.Models.Authentication;

namespace dosyayonetim.api.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ApplicationUser, UserDto>();
            CreateMap<RegisterModel, ApplicationUser>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.Username));
            CreateMap<FileEntity, FileDto>();
            CreateMap<FileShareLink, ShareLinkDto>()
                .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.File.FileName));
        }
    }
} 