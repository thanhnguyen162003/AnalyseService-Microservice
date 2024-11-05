using System.Reflection;
using Application.Common.Behaviours;
using Application.Common.Interfaces.AWS3ServiceInterface;
using Application.Common.Interfaces.ClaimInterface;
using Application.Common.Interfaces.CloudinaryInterface;
using Application.Common.Interfaces.KafkaInterface;
using Application.Common.Models;
using Application.Common.Models.RoadmapDataModel;
using Application.Common.Ultils;
using Application.Consumer;
using Application.Consumer.RetryConsumer;
using Application.Features.RoadmapFeature.Validators;
using Application.Features.SubjectFeature.EventHandler;
using Application.Infrastructure;
using Application.Services;
using Application.Services.MaintainService;
using Application.Services.Search;
using Infrastructure.Data;
using Microsoft.OpenApi.Models;
using Quartz;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddHostedService<UserDataAnalyseConsumer>();
        services.AddHostedService<UserRoadmapGenConsumer>();
        services.AddHostedService<UserDataAnalyseRetryConsumer>();
        services.AddHostedService<UserRoadmapGenRetryConsumer>();
        services.AddHostedService<ConsumerAnalyseService>();
        // services.AddHostedService<RoadmapMissedMaintainService>();
        //services.AddQuartz(configure =>
        //{
        //    var jobKey = new JobKey(nameof(ProcessOutboxMessagesJob));

        //    configure
        //        .AddJob<ProcessOutboxMessagesJob>(jobKey)
        //        .AddTrigger(
        //            trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
        //                schedule => schedule.WithIntervalInHours(10).RepeatForever()));

        //    configure.UseMicrosoftDependencyInjectionJobFactory();
        //});

        //services.AddQuartzHostedService(options =>
        //{
        //    options.WaitForJobsToComplete = true;
        //});
        // BsonSerializer.RegisterSerializer(typeof(ICollection<Guid>), new ICollectionGuidSerializer());
        //Inject Service, Repo, etc...
        services.AddSingleton<AnalyseDbContext>();
        services.AddScoped<IClaimInterface, ClaimService>();
        services.AddSingleton<IProducerService, ProducerService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddScoped<IAWSS3Service, AWSS3Service>();
        services.AddScoped<ISearchService, SearchService>();

        
        //validator
        services.AddScoped<IValidator<RoadMapSectionCreateRequestModel>, CreateRoadmapSectionCommandValidator>();
        services.AddScoped<IValidator<RoadmapCreateRequestModel>, CreateRoadmapCommandValidator>();
        services.AddScoped(typeof(ValidationHelper<>));

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.RegisterServicesFromAssemblyContaining<Program>();
            cfg.AddOpenBehavior(typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddOpenBehavior(typeof(ValidationBehaviour<,>));
        });
        services.AddHttpContextAccessor();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Analyse API", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type=ReferenceType.SecurityScheme,
                            Id="Bearer"
                        }
                    },
                    new string[]{}
                }
            });
        });

        return services;
    }

}
