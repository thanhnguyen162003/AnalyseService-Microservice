
using Application.Common.Models;
using Domain.Entities;
using SharedProject.Models;

namespace Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RoadMapSectionCreateRequestModel, Section>().ReverseMap();
        
        CreateMap<UserAnalyseEntity, UserDataAnalyseModel>().ReverseMap();
    }
}
