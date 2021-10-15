using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TasksObserver.Abstractions;

namespace TasksObserver.UseCases.Tasks.ObserveTasks
{
    public class ObserveTasksCommandHandler : AsyncRequestHandler<ObserveTasksCommand>
    {
        private readonly IMailObserver _mailObserver;

        public ObserveTasksCommandHandler(IMailObserver mailObserver)
        {
            _mailObserver = mailObserver;
        }

        protected override async Task Handle(ObserveTasksCommand request, CancellationToken cancellationToken)
        {
            var newMails = await _mailObserver.GetRecentEmailsAsync(cancellationToken);
            //TODO: Add sending to JIRA.
        }
    }
}