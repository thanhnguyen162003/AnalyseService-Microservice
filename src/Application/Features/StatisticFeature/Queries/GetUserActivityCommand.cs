using System.Collections.Generic;
using System.Threading;
using Amazon.Runtime.Internal.Transform;
using Application.Common.Models.StatisticModel;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using static System.Runtime.InteropServices.JavaScript.JSType;

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
            var currentDate = DateTime.Now;
           
            List<UserActivityModel> list = new List<UserActivityModel>();
            List<DateTime> dates = new List<DateTime>();
            List<UserActivityResponseModel> result = new List<UserActivityResponseModel>();
            switch (request.UserActivityType.ToLower())
            {
                case "year":
                       
                    if (request.IsCountFrom == true)
                    {
                        var startDate = 12 * request.Amount;
                        dates = Enumerable.Range(0, startDate)
                            .Select(i => DateTime.Now.AddMonths(-i)).Reverse()
                            .ToList();
                        var endYear = request.IsCountFrom == true ? DateTime.Now : DateTime.Now.AddYears(-1);
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= new DateTime(DateTime.Now.AddMonths(-startDate).Year, DateTime.Now.AddMonths(-startDate).Month, 1, 0, 0, 0) && x.Date < endYear).ToListAsync();
                    }
                    else
                    {
                        var startDate = DateTime.Now.Year - request.Amount;
                        var end = request.Amount == 0 ? DateTime.Now.Year : DateTime.Now.Year - 1;
                        dates = Enumerable.Range(DateTime.Now.Year - request.Amount, end - startDate + 1)
                            .SelectMany(year => Enumerable.Range(1, 12).Select(month => new DateTime(year, month, 1)))
                            .ToList();
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= new DateTime(startDate, 1, 1, 0, 0, 0) && x.Date < new DateTime(end, 12, 31, 0, 0, 0)).ToListAsync();
                    }
                    result.AddRange(dates.Select(date =>
                    {
                        var key = new DateTime(date.Year, date.Month, 1);
                        return new UserActivityResponseModel
                        {
                            Date = key,
                            Students = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month).Sum(x => x.Students),
                            Teachers = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month).Sum(x => x.Teachers),
                            Moderators = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month).Sum(x => x.Moderators)
                        };
                    }).ToList());
                    break;

                case "month":
                    if (request.IsCountFrom == true)
                    {
                        dates = Enumerable.Range(0, 30 * request.Amount + 1)
                        .Select(i => (DateTime.Now.AddDays(-i))).Reverse()
                        .ToList();
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= new DateTime(DateTime.Now.AddMonths(-1 * request.Amount).Year, DateTime.Now.AddMonths(-1 * request.Amount).Month, 1, 0, 0, 0) && x.Date < DateTime.Now).ToListAsync();
                    }
                    else
                    {
                        var startMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).AddMonths(-request.Amount);
                        var endMonth = request.Amount == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month)) : new DateTime(DateTime.Now.Year, DateTime.Now.Month - 1, DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month - 1));

                        // Generate months from startMonth to current month
                        dates = Enumerable.Range(0, (endMonth - startMonth).Days + 1)
                            .Select(i => startMonth.AddDays(i))
                            .ToList();
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= new DateTime(startMonth.Year, startMonth.Month, 1, 0,0,0) && x.Date < new DateTime(endMonth.Year, endMonth.Month, endMonth.Day, 23,59,59)).ToListAsync();
                    }
                    result.AddRange(dates.Select(date =>
                    {
                        var key = new DateTime(date.Year, date.Month, date.Day);
                        return new UserActivityResponseModel
                        {
                            Date = key,
                            Students = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Students),
                            Teachers = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Teachers),
                            Moderators = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Moderators),
                        };
                    }).ToList());
                    break;

                case "week":
                    if (request.IsCountFrom == false)
                    {
                        var startDate = DateTime.Now.AddDays(-7 * request.Amount);
                        dates = Enumerable.Range(0, (7 * request.Amount))
                            .Select(i => (startDate.AddDays(i)))
                            .ToList();
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= new DateTime(startDate.Year, startDate.Month, 1, 0,0,0 ) && x.Date < DateTime.Now).ToListAsync();
                    }
                    else
                    {
                        var startWeek = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(-7 * request.Amount).Day, 0, 0, 0);
                        var endWeek = request.Amount == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(-1).Day, 23, 59, 59);

                        // Generate weeks from startWeek to current week
                        dates = Enumerable.Range(0, (endWeek - startWeek).Days + 1)
                            .Select(i => startWeek.AddDays(i))
                            .ToList();
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= startWeek && x.Date < endWeek).ToListAsync();
                    }
                    result.AddRange(dates.Select(date =>
                    {

                        var key = new DateTime(date.Year, date.Month, date.Day);
                        return new UserActivityResponseModel
                        {
                            Date = key,
                            Students = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Students),
                            Teachers = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Teachers),
                            Moderators = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Moderators),
                        };
                    }).ToList());
                    break;

                case "day":
                    if (request.IsCountFrom == true)
                    {
                        var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(-request.Amount).Day, 0, 0, 0);
                        dates = Enumerable.Range(0, request.Amount + 1)
                            .Select(i => (startDate.AddDays(i)))
                            .ToList();
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= new DateTime(startDate.Year, startDate.Month, startDate.Day, 0, 0, 0, 0) && x.Date < DateTime.Now).ToListAsync();
                    }
                    else
                    {
                        var startWeek = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(-request.Amount).Day,0,0,0);
                        var endWeek = request.Amount == 0 ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 59) : new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.AddDays(-1).Day, 23, 59, 59);

                        // Generate weeks from startWeek to current week
                        dates = Enumerable.Range(0, (endWeek - startWeek).Days + 1)
                            .Select(i => startWeek.AddDays(i))
                            .ToList();
                        list = await dbContext.UserActivityModel.Find(x => x.Date >= startWeek && x.Date < endWeek).ToListAsync();
                    }
                    result.AddRange(dates.Select(date =>
                    {

                        var key = new DateTime(date.Year, date.Month, date.Day);
                        return new UserActivityResponseModel
                        {
                            Date = key,
                            Students = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Students),
                            Teachers = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Teachers),
                            Moderators = list.Where(x => x.Date.Year == date.Year && x.Date.Month == date.Month && x.Date.Day == date.Day).Sum(x => x.Moderators),
                        };
                    }).ToList());
                    break;

                default:
                    // If type is not recognized, return empty result or throw exception
                    return new List<UserActivityResponseModel>();
            }
            return result;
            
        }
    }
}
