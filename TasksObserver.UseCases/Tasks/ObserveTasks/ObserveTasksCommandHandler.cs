using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using TasksObserver.Abstractions;
using TasksObserver.Abstractions.Models;

namespace TasksObserver.UseCases.Tasks.ObserveTasks
{
    public class ObserveTasksCommandHandler : AsyncRequestHandler<ObserveTasksCommand>
    {
        private readonly IMailObserver _mailObserver;
        private readonly ITaskManager _taskManager;

        public ObserveTasksCommandHandler(IMailObserver mailObserver, ITaskManager taskManager)
        {
            _mailObserver = mailObserver;
            _taskManager = taskManager;
        }

        protected override async Task Handle(ObserveTasksCommand request, CancellationToken cancellationToken)
        {
            var newMails = await _mailObserver.GetRecentEmailsAsync(cancellationToken);

            var issues = newMails.Select(mail => new Issue
            {
                Title = mail.Subject,
                Text = mail.Message,
                ContactAddress = mail.From
            });

            var issuesSendingTasks = issues.Select(async issue =>
                await _taskManager.SendIssueAsync(issue, cancellationToken));

            await Task.WhenAll(issuesSendingTasks);
        }
    }
}