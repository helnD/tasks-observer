using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TasksObserver.Abstractions.Models;

namespace TasksObserver.Abstractions
{
    public interface IHandledMailsProvider
    {
        Task<bool> IsHandledAsync(uint id, CancellationToken cancellationToken);

        Task AddNewMailsAsync(IEnumerable<Mail> newMails, CancellationToken cancellationToken);
    }
}