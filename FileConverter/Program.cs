using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;

namespace FileConverter
{
    public class Program
    {
        #region Properties and Variables      
           
        //private static ILogger _logger;
        private static IConfigurationRoot _config;
        #endregion

        private static void Main(string[] args) 
        {
            // Create service collection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            // Create service provider
            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Run app
            serviceProvider.GetService<FFV2CSVConverter>().Run();

            Console.WriteLine("Press any key to exit");
            Console.ReadLine();
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Build configuration
            _config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", false)
                .Build();

            serviceCollection.AddSingleton(_config);

            serviceCollection.AddLogging(configure => configure.AddConsole())
            .Configure<LoggerFilterOptions>(options => options.MinLevel = LogLevel.Information)
            .AddTransient<FFV2CSVConverter>();
        }
    }
}