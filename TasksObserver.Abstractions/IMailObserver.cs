using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TasksObserver.Abstractions.Models;

namespace TasksObserver.Abstractions
{
    public interface IMailObserver
    {
        Task<IEnumerable<Mail>> GetRecentEmailsAsync(CancellationToken cancellationToken);
    }
}