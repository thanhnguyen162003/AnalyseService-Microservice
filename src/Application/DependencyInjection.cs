using System.Reflection;
using Application.Common.Behaviours;
using Application.Common.Interfaces.AWS3ServiceInterface;
using Application.Common.Interfaces.ClaimInterface;
using Application.Common.Interfaces.CloudinaryInterface;
using Application.Common.Interfaces.KafkaInterface;
using Application.Common.Models;
using Application.Common.Ultils;
using Application.Features.RoadmapFeature.Validators;
using Application.Infrastructure;
using Application.Services;
using Infrastructure.Data;
using Microsoft.OpenApi.Models;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        // services.AddHostedService<SearchDataConsumer>();

        //Inject Service, Repo, etc...
        services.AddSingleton<AnalyseDbContext>();
        services.AddScoped<IClaimInterface, ClaimService>();
        services.AddSingleton<IProducerService, ProducerService>();
        services.AddScoped<ICloudinaryService, CloudinaryService>();
        // services.AddScoped<ImageToPdfHelper>();
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddScoped<IAWSS3Service, AWSS3Service>();
        
        //validator
        services.AddScoped<IValidator<RoadMapSectionCreateRequestModel>, CreateRoadmapSectionCommandValidator>();
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
