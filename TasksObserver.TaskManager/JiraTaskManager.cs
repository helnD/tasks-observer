using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dapplo.Jira;
using Dapplo.Jira.Entities;
using Microsoft.Extensions.Options;
using TasksObserver.Abstractions;
using TasksObserver.Infrastructure;
using Issue = TasksObserver.Abstractions.Models.Issue;

namespace TasksObserver.TaskManager
{
    public class JiraTaskManager : ITaskManager
    {
        private readonly IJiraClient _jiraClient;
        private readonly JiraSettings _jiraSettings;

        public JiraTaskManager(IOptions<JiraSettings> jiraSettings)
        {
            _jiraSettings = jiraSettings.Value;
            _jiraClient = JiraClient.Create(new Uri(_jiraSettings.JiraHost));
            _jiraClient.SetBasicAuthentication(_jiraSettings.JiraClient, _jiraSettings.JiraToken);
        }

        public async Task SendIssueAsync(Issue issue, CancellationToken cancellationToken)
        {
            var jiraIssue = new IssueWithFields<IssueFields>
            {
                Fields = new()
                {
                    Summary = issue.Title,
                    Description = issue.Text,
                    Project = new Project
                    {
                        Key = _jiraSettings.JiraProject
                    },
                    IssueType = new IssueType
                    {
                        Name = "Task"
                    }
                }
            };

            await _jiraClient.Issue.CreateAsync(jiraIssue, cancellationToken);
        }
    }
}