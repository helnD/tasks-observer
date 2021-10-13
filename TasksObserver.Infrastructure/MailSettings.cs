using System;

namespace TasksObserver.Infrastructure
{
    public class MailSettings
    {
        public string Domain { get; init; }

        public int Port { get; init; }

        public string Login { get; init; }

        public string Password { get; init; }
    }
}