﻿namespace TasksObserver.Infrastructure
{
    public class AppSettings
    {
        public int UpdateFrequencyInMinutes { get; init; }

        public string ChangesRequestSuffix { get; init; }
    }
}