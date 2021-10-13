namespace TasksObserver.Abstractions.Models
{
    public class Mail
    {
        public uint Id { get; init; }

        public string From { get; init; }

        public string To { get; init; }

        public string Subject { get; init; }

        public string Message { get; init; }
    }
}