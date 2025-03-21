using Application.Common.Models.StatisticModel;
using Domain.Enums;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Application.Features.StatisticFeature.Queries
{
    public class GetUserActivityCommand : IRequest<List<UserActivityResponseModel>>
    {
        public int Amount { get; set; }
        public string UserActivityType { get; set; }
        public bool IsCountFrom { get; set; }
    }

    public class GetUserActivityCommandCHandler(AnalyseDbContext dbContext, IMapper _mapper) : IRequestHandler<GetUserActivityCommand, List<UserActivityResponseModel>>
    {
        public async Task<List<UserActivityResponseModel>> Handle(GetUserActivityCommand request, CancellationToken cancellationToken)
        {
            var list = dbContext.UserActivityModel.Find(x => x.Date <= new DateTime(2025,3,30)).ToList();
            var test = _mapper.Map<List<UserActivityResponseModel>>(list);
            return test;
        }
    }
}
