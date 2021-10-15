using System.Threading;
using System.Threading.Tasks;
using TasksObserver.Abstractions.Models;

namespace TasksObserver.Abstractions
{
    public interface ITaskManager
    {
        Task SendIssueAsync(Issue issue, CancellationToken cancellationToken);
    }
}