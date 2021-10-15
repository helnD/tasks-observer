using System;
using System.IO;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TasksObserver.Abstractions;
using TasksObserver.Infrastructure;
using TasksObserver.MailObserver;
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

            serviceCollection.Configure<MailSettings>(_configuration.GetSection("MailSettings"));
            serviceCollection.Configure<AppSettings>(_configuration.GetSection("AppSettings"));
            serviceCollection.AddSingleton<IHandledMailsProvider, HandledMailsProvider.HandledMailsProvider>();
            serviceCollection.AddTransient<IMailObserver, MailKitObserver>();
            serviceCollection.AddTransient<TasksObserverService>();

            serviceCollection.AddMediatR(typeof(ObserveTasksCommand).Assembly);

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