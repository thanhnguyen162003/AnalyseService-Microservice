using Algolia.Search.Http;
using Application;
using Application.Common.Models.SearchModel;
using Application.Common.Models.StatisticModel;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using static Application.FlashcardServiceRpc;
using static Application.UserServiceRpc;

public class FiveMinutesTaskService : BackgroundService
{
    private readonly FlashcardServiceRpc.FlashcardServiceRpcClient _flashcardServiceRpcClient;
    private readonly AnalyseDbContext _dbContext;
    private readonly EnrollmentServiceRpc.EnrollmentServiceRpcClient _enrollmentService;
    public FiveMinutesTaskService(FlashcardServiceRpc.FlashcardServiceRpcClient flashcardServiceRpcClient, EnrollmentServiceRpc.EnrollmentServiceRpcClient enrollmentService, AnalyseDbContext dbContext)
    {
        _flashcardServiceRpcClient = flashcardServiceRpcClient ?? throw new ArgumentNullException(nameof(flashcardServiceRpcClient));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _enrollmentService = enrollmentService ?? throw new ArgumentNullException(nameof(enrollmentService));
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await AddUserFlashcardLearning();
                await AddUserLessonLearning();
            }
            catch (Exception ex)
            {
                // Log the error (if logging is available)
                Console.WriteLine($"Error in scheduled task: {ex.Message}");
            }

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
    private async Task AddUserLessonLearning()
    {
        try
        {
            var response = await _enrollmentService.GetEnrollmentAsync(new EnrollmentRequest());
            var today = DateTime.Today; // Get current date for comparison

            foreach (var enrollment in response.Enrollment)
            {
                var lessonLearnDates = enrollment.LessonLearnDate; // repeated string from proto
                if (lessonLearnDates.Count == 0) continue;

                var userIdGuid = Guid.Parse(enrollment.UserId);
                var filter = Builders<UserLessonLearningModel>.Filter.Eq(u => u.UserId, userIdGuid);
                var existingUser = await _dbContext.UserLessonLearningModel.Find(filter).FirstOrDefaultAsync();

                // Convert lessonLearnDates strings to DateTime and filter today's lessons
                var parsedDates = lessonLearnDates.Select(d => DateTime.Parse(d)).ToList();
                var todayLessonsCount = parsedDates.Count(d => d.Date == today);

                if (existingUser != null)
                {
                    // Calculate the difference based on today's count
                    int oldToday = existingUser.TodayLessonsLearned;
                    int diff = todayLessonsCount - oldToday;

                    if (diff != 0 || !existingUser.LearningDates.SequenceEqual(parsedDates))
                    {
                        var update = Builders<UserLessonLearningModel>.Update
                            .Set(u => u.TodayLessonsLearned, todayLessonsCount)
                            .Set(u => u.LearningDates, parsedDates)
                            .Inc(u => u.TotalLessonsLearned, diff);

                        await _dbContext.UserLessonLearningModel.UpdateOneAsync(filter, update);
                    }
                }
                else
                {
                    // Create new user record
                    var newUser = new UserLessonLearningModel
                    {
                        Id = ObjectId.GenerateNewId().ToString(),
                        UserId = userIdGuid,
                        TodayLessonsLearned = todayLessonsCount,
                        TotalLessonsLearned = parsedDates.Count, // Total is all lessons, not just today's
                        LearningDates = parsedDates
                    };

                    await _dbContext.UserLessonLearningModel.InsertOneAsync(newUser);
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    private async Task AddUserFlashcardLearning()
    {
        try
        {
            var list = await _flashcardServiceRpcClient.GetUserFlashcardLearningAsync(new UserFlashcardLearningRequest());

            var response = list.UserFlashcardLearning
                .GroupBy(x => new { x.FlashcardId, x.FlashcardContentId })
                .Select(group => new UserFlashcardLearningModel
                {
                    FlashcardId = Guid.Parse(group.Key.FlashcardId),
                    FlashcardContentId = Guid.Parse(group.Key.FlashcardContentId),
                    LearningDates = group
                        .SelectMany(x => x.LastReviewDateHistory.Select(DateTime.Parse))
                        .OrderBy(date => date)
                        .ToList(),
                    UserId = Guid.Parse(group.First().UserId),
                    TimeSpentHistory = group.SelectMany(x => x.TimeSpentHistory).ToList()

                })
                .ToList();

            var flashcardIds = response.Select(x => x.FlashcardId).ToList();
            var flashcardContentIds = response.Select(x => x.FlashcardContentId).ToList();

            var dbRecords = await _dbContext.UserFlashcardLearningModel
                .Find(x => flashcardIds.Contains(x.FlashcardId) && flashcardContentIds.Contains(x.FlashcardContentId))
                .ToListAsync();

            // Convert DB records into a dictionary for quick lookup
            var dbRecordsDict = dbRecords.ToDictionary(x => (x.FlashcardId, x.FlashcardContentId, x.UserId));

            // Lists for new inserts and updates
            List<UserFlashcardLearningModel> newRecords = new();
            List<ReplaceOneModel<UserFlashcardLearningModel>> updates = new();

            foreach (var record in response)
            {
                var key = (record.FlashcardId, record.FlashcardContentId, record.UserId);

                if (dbRecordsDict.TryGetValue(key, out var existingRecord))
                {
                    // Check if data is different
                    var test = !AreRecordsEqual(existingRecord, record);
                    if (!AreRecordsEqual(existingRecord, record))
                    {
                        record.Id = existingRecord.Id;
                        updates.Add(new ReplaceOneModel<UserFlashcardLearningModel>(
                            Builders<UserFlashcardLearningModel>.Filter.Eq(x => x.Id, existingRecord.Id),
                            record)
                        {
                            IsUpsert = false // Only update existing records
                        });
                    }
                }
                else
                {
                    // If record doesn't exist, add it as a new entry
                    newRecords.Add(record);
                }
            }

            // Batch update existing records
            if (updates.Any())
            {
                await _dbContext.UserFlashcardLearningModel.BulkWriteAsync(updates);
            }

            // Insert new records
            if (newRecords.Any())
            {
                await _dbContext.UserFlashcardLearningModel.InsertManyAsync(newRecords);
            }

        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
    public bool AreRecordsEqual(UserFlashcardLearningModel dbRecord, UserFlashcardLearningModel newRecord)
    {
        var test = dbRecord.FlashcardId == newRecord.FlashcardId;
        var test2 = dbRecord.FlashcardContentId == newRecord.FlashcardContentId;
        var test3 = dbRecord.UserId == newRecord.UserId;
        var test4 = dbRecord.LearningDates.Count == newRecord.LearningDates.Count;
        var test5 = dbRecord.LearningDates
    .Select(d => d.ToString("yyyy-MM-dd HH:mm:ss")) // Ignores milliseconds
    .SequenceEqual(newRecord.LearningDates.Select(d => d.ToString("yyyy-MM-dd HH:mm:ss")));

        return dbRecord.FlashcardId == newRecord.FlashcardId &&
               dbRecord.FlashcardContentId == newRecord.FlashcardContentId &&
               dbRecord.UserId == newRecord.UserId &&
               dbRecord.LearningDates.Count == newRecord.LearningDates.Count &&
               dbRecord.LearningDates
    .Select(d => d.ToString("yyyy-MM-dd HH:mm:ss")) // Ignores milliseconds
    .SequenceEqual(newRecord.LearningDates.Select(d => d.ToString("yyyy-MM-dd HH:mm:ss")))
               ;
    }

}
