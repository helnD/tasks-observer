using System;

namespace TasksObserver.Domain
{
    public class ChangesRequest
    {
        public string Title { get; init; }

        public string Description { get; init; }

        public ChangesRequestType RequestType { get; init; }
    }
}