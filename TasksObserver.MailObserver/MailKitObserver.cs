using System;
using System.Collections.Generic;
using System.Linq;
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

            var requestsFolder = await GetRequestsFolderAsync(cancellationToken);
            var todayQuery = SearchQuery.DeliveredAfter(DateTime.Now.Date);

            var messages = await requestsFolder.SearchAsync(todayQuery, cancellationToken);
            var notHandledMessages = await GetNotHandledMessagesAsync(messages, cancellationToken);

            var messageInfo = await GetMessagesAsync(notHandledMessages, requestsFolder, cancellationToken);

            var messagesWithTexts = notHandledMessages.Zip(messageInfo);
            var resultMails = messagesWithTexts.Select(message => new Mail
            {
                Id = message.First.Id,
                From = GetAddressFromAddressList(message.Second.From),
                To = GetAddressFromAddressList(message.Second.To),
                Subject = message.Second.Subject,
                Message = message.Second.HtmlBody
            }).ToList();

            await _imapClient.DisconnectAsync(true, cancellationToken);
            await _handledMailsProvider.AddNewMailsAsync(resultMails, cancellationToken);
            return resultMails;
        }

        private async Task<IEnumerable<MimeMessage>> GetMessagesAsync(IEnumerable<UniqueId> messageIds, IMailFolder folder,
            CancellationToken cancellationToken)
        {
            var messages = new List<MimeMessage>();
            foreach (var messageId in messageIds)
            {
                var message = await folder.GetMessageAsync(messageId, cancellationToken);
                messages.Add(message);
            }

            return messages;
        }

        private async Task<IMailFolder> GetRequestsFolderAsync(CancellationToken cancellationToken)
        {
            var requestsFolder = await _imapClient.GetFolderAsync(_appSettings.ChangeRequestsFolder,  cancellationToken);
            await requestsFolder.OpenAsync(FolderAccess.ReadOnly, cancellationToken);
            return requestsFolder;
        }

        private async Task<IEnumerable<UniqueId>> GetNotHandledMessagesAsync(IEnumerable<UniqueId> folderMessages,
            CancellationToken cancellationToken)
        {
            var notHandledMessages = new List<UniqueId>();
            foreach (var message in folderMessages)
            {
                if (await _handledMailsProvider.IsHandledAsync(message.Id, cancellationToken))
                {
                    continue;
                }

                notHandledMessages.Add(message);
            }

            return notHandledMessages;
        }

        private string GetAddressFromAddressList(InternetAddressList addressList) =>
            addressList.OfType<MailboxAddress>().FirstOrDefault()?.Address ?? "none";

        public void Dispose()
        {
            _imapClient?.Dispose();
        }
    }
}