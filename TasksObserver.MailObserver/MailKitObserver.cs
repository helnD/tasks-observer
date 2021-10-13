using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Threading;
using System.Threading.Tasks;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using Microsoft.Extensions.Options;
using MimeKit;
using TasksObserver.Abstractions;
using TasksObserver.Abstractions.Models;
using TasksObserver.Infrastructure;

namespace TasksObserver.MailObserver
{
    public class MailKitObserver : IMailObserver, IDisposable
    {
        private readonly ImapClient _imapClient;
        private readonly MailSettings _mailSettings;
        private readonly AppSettings _appSettings;
        private readonly IHandledMailsProvider _handledMailsProvider;

        public MailKitObserver(IOptions<MailSettings> mailSettings,
            IHandledMailsProvider handledMailsProvider,
            IOptions<AppSettings> appSettings)
        {
            _handledMailsProvider = handledMailsProvider;
            _appSettings = appSettings.Value;
            _mailSettings = mailSettings.Value;
            _imapClient = new ImapClient();
        }

        public async Task<IEnumerable<Mail>> GetRecentEmailsAsync(CancellationToken cancellationToken)
        {
            await _imapClient.ConnectAsync(_mailSettings.Domain, _mailSettings.Port, true, cancellationToken);
            await _imapClient.AuthenticateAsync(_mailSettings.Login, _mailSettings.Password, cancellationToken);
            await _imapClient.Inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

            var messagesToFetch = new List<UniqueId>();

            var emailAfterFilter = SearchQuery.DeliveredAfter(
                DateTime.Now.AddMinutes(-3 * _appSettings.UpdateFrequencyInMinutes));
            var recentMessages = await _imapClient.Inbox.SearchAsync(emailAfterFilter, cancellationToken);

            foreach (var messageId in recentMessages)
            {
                if (await _handledMailsProvider.IsHandledAsync(messageId.Id, cancellationToken))
                {
                    continue;
                }

                messagesToFetch.Add(messageId);
            }

            var fetchedMessages = await _imapClient.Inbox
                .FetchAsync(messagesToFetch, MessageSummaryItems.All, cancellationToken);
            var onlyProcessableMessages = GetOnlyProcessableMessages(fetchedMessages)
                .ToList();

            var messageTexts = new List<string>();
            foreach (var message in onlyProcessableMessages)
            {
                var newText = await GetMessageText(message.UniqueId, cancellationToken);
                messageTexts.Add(newText);
            }

            await _imapClient.DisconnectAsync(true, cancellationToken);

            var messagesWithTexts = onlyProcessableMessages.Zip(messageTexts);
            var resultMails = messagesWithTexts.Select(message => new Mail
            {
                Id = message.First.UniqueId.Id,
                From = message.First.Envelope.From.OfType<MailboxAddress>().First().Address,
                To = message.First.Envelope.To.OfType<MailboxAddress>().First().Address,
                Subject = message.First.Envelope.Subject,
                Message = message.Second
            }).ToList();

            await _handledMailsProvider.AddNewMailsAsync(resultMails, cancellationToken);
            return resultMails;
        }

        private async Task<string> GetMessageText(UniqueId messageId, CancellationToken cancellationToken)
        {
            var message = await _imapClient.Inbox.GetMessageAsync(messageId, cancellationToken);
            return message.TextBody;
        }

        private IEnumerable<IMessageSummary> GetOnlyProcessableMessages(IEnumerable<IMessageSummary> messageSummaries)
        {
            return messageSummaries.Where(summary => GetAddressFromAddressList(summary.Envelope.To)
                .Contains(_appSettings.ChangesRequestSuffix));
        }

        private string GetAddressFromAddressList(InternetAddressList addressList) =>
            addressList.OfType<MailboxAddress>().FirstOrDefault()?.Address ?? "none";

        public void Dispose()
        {
            _imapClient?.Dispose();
        }
    }
}