using HotelRoomAvailability.Repositories.Abstractions;
using HotelRoomAvailability.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace HotelRoomAvailability.Infrastructure;

internal static class ServiceCollectionExtensions
{
    internal static IServiceCollection ConfigureInfrastructureServices(this IServiceCollection services)
    {
        services.Scan(scan => scan
            .FromAssemblyOf<IHotelsRepository>()
            .AddClasses(classes => classes.Where(type => type.Name.EndsWith("Repository")))
            .AsImplementedInterfaces()
            .WithScopedLifetime());

        services.AddScoped<IAvailabilityService, AvailabilityService>();
        services.AddScoped<IApplication, Application>();
        services.AddMemoryCache();
        return services;
    }

    internal static IServiceCollection ConfigureLogging(this IServiceCollection services)
        => services.AddLogging(builder =>
        {
            builder.ClearProviders().AddNLog("nlog.config");
        });
}
