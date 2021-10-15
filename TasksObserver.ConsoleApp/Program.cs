using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TasksObserver.Abstractions;
using TasksObserver.Infrastructure;
using TasksObserver.MailObserver;
using TasksObserver.TaskManager;
using TasksObserver.UseCases.Tasks.ObserveTasks;

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

            // Logs
            var serilogLogger = new LoggerConfiguration()
                .ReadFrom.Configuration(_configuration)
                .CreateLogger();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog(logger: serilogLogger, dispose: true);
            });

            // Settings.
            serviceCollection.Configure<MailSettings>(_configuration.GetSection("MailSettings"));
            serviceCollection.Configure<AppSettings>(_configuration.GetSection("AppSettings"));
            serviceCollection.Configure<JiraSettings>(_configuration.GetSection("JiraSettings"));

            // Custom services.
            serviceCollection.AddSingleton<IHandledMailsProvider, HandledMailsProvider.HandledMailsProvider>();
            serviceCollection.AddTransient<IMailObserver, MailKitObserver>();
            serviceCollection.AddTransient<ITaskManager, JiraTaskManager>();
            serviceCollection.AddTransient<TasksObserverService>();

            // MediatR.
            serviceCollection.AddMediatR(typeof(ObserveTasksCommand).Assembly);

            // hangfire.
            serviceCollection.AddHangfire((serviceProvider, config) =>
                config.UseMemoryStorage()
                    .UseActivator(new HangfireActivator(serviceProvider)));
        }

        static async Task Startup()
        {
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            await using var serviceProvider = serviceCollection.BuildServiceProvider();
            var tasksObserver = serviceProvider.GetService<TasksObserverService>();

            await tasksObserver.StartObservation();
        }
    }
}