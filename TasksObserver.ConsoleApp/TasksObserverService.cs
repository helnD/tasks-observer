using System.Threading;
using System.Threading.Tasks;
using Hangfire;
using MediatR;
using TasksObserver.UseCases.Tasks.ObserveTasks;

namespace TasksObserver.ConsoleApp
{
    public class TasksObserverService
    {
        private readonly BackgroundJobServer server;
        private readonly IMediator _mediator;
        private readonly CancellationToken _cancellationToken;
        private readonly IRecurringJobManager _recurringJobManager;

        public TasksObserverService(IMediator mediator, IRecurringJobManager recurringJobManager)
        {
            _mediator = mediator;
            _recurringJobManager = recurringJobManager;
            _cancellationToken = new CancellationTokenSource().Token;
            server = new BackgroundJobServer(new BackgroundJobServerOptions
            {
                WorkerCount = 3
            });
        }

        public async Task StartObservation()
        {
            _recurringJobManager.AddOrUpdate("MailJob", () => CallTasksObservation(), Cron.Minutely);
            _recurringJobManager.Trigger("MailJob");
            await server.WaitForShutdownAsync(_cancellationToken);
        }

        public async Task CallTasksObservation()
        {
            await _mediator.Send(new ObserveTasksCommand(), _cancellationToken);
        }
    }
}