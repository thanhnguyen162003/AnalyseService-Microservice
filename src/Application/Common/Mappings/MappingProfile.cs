
using Application.Common.Models;
using Application.Common.Models.RoadmapDataModel;
using Domain.Entities;
using SharedProject.Models;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RoadMapSectionCreateRequestModel, Roadmap>().ReverseMap();
        
        CreateMap<RoadmapResponseModel, Roadmap>().ReverseMap();
        
        CreateMap<RoadmapDetailResponseModel, Roadmap>().ReverseMap();
        
        CreateMap<UserDataAnalyseModel, UserAnalyseEntity>()
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));
    }
}
