using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TasksObserver.Abstractions;
using TasksObserver.Infrastructure;
using TasksObserver.MailObserver;

namespace TasksObserver.ConsoleApp
{
    class Program
    {
        private static IConfiguration _configuration;

        static async Task Main(string[] args)
        {
            await Startup();
        }

        static void ConfigureServices(IServiceCollection serviceCollection)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");

            _configuration = configurationBuilder.Build();

            serviceCollection.Configure<MailSettings>(_configuration.GetSection("MailSettings"));
            serviceCollection.Configure<AppSettings>(_configuration.GetSection("AppSettings"));
            serviceCollection.AddSingleton<IHandledMailsProvider, HandledMailsProvider.HandledMailsProvider>();
            serviceCollection.AddTransient<IMailObserver, MailKitObserver>();
        }

        static async Task Startup()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            var mailService = serviceProvider.GetService<IMailObserver>();
        }
    }
}