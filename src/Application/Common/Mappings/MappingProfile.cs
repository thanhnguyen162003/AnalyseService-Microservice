
using Application.Common.Models;
using Application.Common.Models.RoadmapDataModel;
using Application.Common.Models.StatisticModel;
using Domain.Entities;
using SharedProject.Models;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RoadMapSectionCreateRequestModel, Roadmap>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RoadmapId))
            .ReverseMap();

        CreateMap<RoadmapCreateRequestModel, Roadmap>().ReverseMap();
        
        CreateMap<RoadmapResponseModel, Roadmap>().ReverseMap();
        
        CreateMap<RoadmapDetailResponseModel, Roadmap>().ReverseMap();
        
        CreateMap<RecentViewModel, RecentView>().ReverseMap();
        CreateMap<UserActivityModel, UserActivityResponseModel>().ReverseMap();

        CreateMap<UserDataAnalyseModel, UserAnalyseEntity>()
            .ForMember(dest => dest.Subjects, opt => opt.MapFrom(src => src.Subjects));
    }
}
