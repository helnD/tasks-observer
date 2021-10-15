using System;
using Hangfire;

namespace TasksObserver.ConsoleApp
{
    /// <summary>
    /// Provides Microsoft DI activator.
    /// </summary>
    public class HangfireActivator : JobActivator
    {
        private readonly IServiceProvider serviceProvider;

        /// <summary>
        /// Constructor.
        /// </summary>
        public HangfireActivator(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Activates job.
        /// </summary>
        /// <param name="jobType">Type of job.</param>
        public override object ActivateJob(Type jobType)
        {
            return serviceProvider.GetService(jobType);
        }
    }
}