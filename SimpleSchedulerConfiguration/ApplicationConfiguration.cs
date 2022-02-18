using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleSchedulerConfiguration.Models;

namespace SimpleSchedulerConfiguration;

public static class ApplicationConfiguration
{
    public static void SetUpConfiguration(ConfigurationManager config)
    {
        config.AddJsonFile("secrets.json", optional: true);
    }

    public static void SetUpAppSettings(IConfiguration config, IServiceCollection serviceCollection)
    {
        AppSettings appSettings = config.GetSection("AppSettings").Get<AppSettings>();
        serviceCollection.AddSingleton(appSettings);
    }

    public static void 
}
