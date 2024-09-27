
using Application.Common.Models;
using Domain.Entities;
using SharedProject.Models;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RoadMapSectionCreateRequestModel, Section>().ReverseMap();
        
        CreateMap<UserDataAnalyseModel, UserAnalyseEntity>()
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));
    }
}
