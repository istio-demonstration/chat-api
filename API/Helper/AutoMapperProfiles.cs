using System.Linq;
using API.DTOs.Member;
using API.DTOs.Photo;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helper
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<AppUser, MemberToReturnDto>()
                .ForMember(dest => dest.MainPhotoUrl, 
                    options => options.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge())); ;
            CreateMap<Photo, PhotoDto>();
        }
    }
}
