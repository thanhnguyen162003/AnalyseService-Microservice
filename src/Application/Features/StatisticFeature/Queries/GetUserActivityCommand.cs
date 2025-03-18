using Application.Common.Models.StatisticModel;
using Domain.E;
using Domain.Enums;
using Infrastructure.Data;
using MongoDB.Driver;

namespace Application.Features.StatisticFeature.Queries
{
    public class GetUserActivityCommand : IRequest<List<UserActivityResponseModel>>
    {
        public int Amount { get; set; }
        public string UserActivityType { get; set; }
    }

    public class GetUserActivityCommandHandler(AnalyseDbContext dbContext, IMapper _mapper) : IRequestHandler<GetUserActivityCommand, List<UserActivityResponseModel>>
    {
        public async Task<List<UserActivityResponseModel>> Handle(GetUserActivityCommand request, CancellationToken cancellationToken)
        {
            List<UserActivityResponseModel> result = new List<UserActivityResponseModel>();
            if (request.UserActivityType.Equals(UserActivityEnum.Year.ToString()))
            {
                var session = await dbContext.SessionRepository.GetSessionStatisticYear(request.Amount);
                if (!session.Any())
                {
                    return new List<UserActivityResponseModel>();
                }
                for (int i = 11; i >= 0; i--)
                {
                    var date = DateTime.Now.AddMonths(-i);
                    var userIds = session
                        .Where(x => x.UpdatedAt.Month == date.Month)
                        .Select(x => x.UserId.Value)
                        .ToList();

                    var count = await _unitOfWork.UserRepository.CountUserByRole(userIds);


                    var student = 0;
                    var teacher = 0;
                    var moderator = 0;
                    foreach (var item in count)
                    {
                        if (item.Key == RoleEnum.Student.ToString())
                        {
                            student = item.Value;
                        }
                        else if (item.Key == RoleEnum.Teacher.ToString())
                        {
                            teacher = item.Value;
                        }
                        else if (item.Key == RoleEnum.Moderator.ToString())
                        {
                            moderator = item.Value;
                        }
                    }
                    UserActivityResponseModel userActivityResponseModel = new UserActivityResponseModel()
                    {
                        Date = new DateTime(DateTime.Now.Year, date.Month, 1),
                        Students = student,
                        Teachers = teacher,
                        Moderators = moderator
                    };
                    result.Add(userActivityResponseModel);
                }

                return result;
            }
            else if (request.UserActivityType.Equals(UserActivityEnum.Month.ToString()))
            {
                var session = await _unitOfWork.SessionRepository.GetSessionStatisticMonth(request.Amount);

                return new List<UserActivityResponseModel>()
                {

                };
            }
            else if (request.UserActivityType.Equals(UserActivityEnum.Week.ToString()))
            {
                var session = await _unitOfWork.SessionRepository.GetSessionStatisticWeek(request.Amount);
                return new List<UserActivityResponseModel>();
            }
            else
            {
                return new List<UserActivityResponseModel>();
            }
        }
    }
}
