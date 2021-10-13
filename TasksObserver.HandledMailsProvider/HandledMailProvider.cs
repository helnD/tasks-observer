using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TasksObserver.Abstractions;
using TasksObserver.Abstractions.Models;

namespace TasksObserver.HandledMailsProvider
{
    public class HandledMailsProvider : IHandledMailsProvider
    {
        private HashSet<Mail> _mails = new();

        public Task<bool> IsHandledAsync(uint id, CancellationToken cancellationToken)
        {
            return Task.FromResult(_mails.Any(mail => mail.Id == id));
        }

        public Task AddNewMailsAsync(IEnumerable<Mail> newMails, CancellationToken cancellationToken)
        {
            foreach (var mail in newMails)
            {
                _mails.Add(mail);
            }

            return Task.CompletedTask;
        }
    }
}