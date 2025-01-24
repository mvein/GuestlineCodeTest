using HotelRoomAvailability;
using HotelRoomAvailability.Infrastructure;
using HotelRoomAvailability.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.ConfigureInfrastructureServices()
            .ConfigureLogging()
            .Configure<CommandLineOptions>(options => { options.Args = args; });
    })
    .Build();

var logger = host.Services.GetRequiredService<ILogger<Program>>();
var application = host.Services.GetRequiredService<IApplication>();

try
{
    logger.LogInformation("Application started");
    await application.Run();
}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred");
}