using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fabron;
using Fabron.Contracts;
using HoneyComb.API.Resources;
using HoneyComb.API.Resources.CronReminders.Models;
using HoneyComb.API.Resources.Reminders.Models;
using HoneyComb.Commands;

using static HoneyComb.Constants;

namespace HoneyComb.API.Services
{
    public interface IReminderManager
    {
        Task DeleteCronReminder(string appId, string name);
        Task DeleteReminder(string appId, string name);
        Task<CronReminder?> GetCronReminder(string appId, string name);
        Task<CronReminderItem?> GetCronReminderItem(string appId, string name, DateTime schedule);
        Task<Reminder?> GetReminder(string appId, string name);
        Task<PaginatedList<CronReminderItem>?> ListCronReminderItems(string appId, string name, int pageIndex, int pageSize);
        Task<PaginatedList<CronReminder>> ListCronReminders(string appId, int pageIndex, int pageSize);
        // Task<PaginatedList<CronReminder>> SearchCronRemindersByDisplayName(string appId, string pattern);
        Task<PaginatedList<Reminder>> ListReminders(string appId, int pageIndex, int pageSize);
        // Task<PaginatedList<Reminder>> SearchRemindersByDisplayName(string appId, string pattern);
        Task<CronReminder> RegisterCronReminder(
            string appId,
            string name,
            string? displayName,
            string schedule,
            InvokeHttpRequest command,
            DateTime? notBefore,
            DateTime? expirationTime,
            bool suspend);
        Task<Reminder> RegisterReminder(
            string appId,
            string name,
            string? displayName,
            DateTime schedule,
            InvokeHttpRequest command);
        Task ResumeCronReminder(string appId, string name);
        Task SuspendCronReminder(string appId, string name);
    }

    public class ReminderManager : IReminderManager
    {
        private readonly IJobManager _jobManager;

        public ReminderManager(
            IJobManager jobManager) => _jobManager = jobManager;

        public async Task<Reminder> RegisterReminder(
            string appId,
            string name,
            string? displayName,
            DateTime schedule,
            InvokeHttpRequest command)
        {
            var key = $"apps/{appId}/Reminders/{name}";

            Dictionary<string, string>? labels = new Dictionary<string, string> {
                { LabelNames.ApplicationId, appId },
                { LabelNames.Name, name },
            };
            if (displayName != null)
            {
                labels.Add(FabronConstants.LabelNames.DisplayName, displayName);
            }
            Job<InvokeHttpRequest, InvokeHttpRequestResult> job =
                await _jobManager.ScheduleJob<InvokeHttpRequest, InvokeHttpRequestResult>(
                key,
                command,
                schedule,
                labels,
                null);
            Reminder reminder = job.ToReminder();
            return reminder;
        }

        public async Task<Reminder?> GetReminder(
            string appId,
            string name)
        {
            var key = $"apps/{appId}/Reminders/{name}";

            Job<InvokeHttpRequest, InvokeHttpRequestResult>? job =
                await _jobManager.GetJob<InvokeHttpRequest, InvokeHttpRequestResult>(key);
            return job?.ToReminder();
        }

        public async Task<PaginatedList<Reminder>> ListReminders(
            string appId,
            int pageIndex,
            int pageSize)
        {
            IEnumerable<Job<InvokeHttpRequest, InvokeHttpRequestResult>> jobs =
                await _jobManager.GetJobByLabel<InvokeHttpRequest, InvokeHttpRequestResult>(LabelNames.ApplicationId, appId);
            List<Reminder> list = jobs.Select(cronJob => cronJob.ToReminder()).ToList();
            return new PaginatedList<Reminder>(list, list.Count, pageIndex, pageSize);
        }

        public async Task DeleteReminder(
            string appId,
            string name)
        {
            var key = $"apps/{appId}/Reminders/{name}";
            await _jobManager.DeleteJob(key);
            return;
        }

        public async Task<CronReminder> RegisterCronReminder(
            string appId,
            string name,
            string? displayName,
            string schedule,
            InvokeHttpRequest command,
            DateTime? notBefore,
            DateTime? expirationTime,
            bool suspend)
        {
            var key = $"apps/{appId}/CronReminders/{name}";

            Dictionary<string, string>? labels = new Dictionary<string, string> {
                { LabelNames.ApplicationId, appId },
                { LabelNames.Name, name },
            };
            if (displayName != null)
            {
                labels.Add(FabronConstants.LabelNames.DisplayName, displayName);
            }
            CronJob<InvokeHttpRequest>? cronJob =
                await _jobManager.ScheduleCronJob<InvokeHttpRequest>(key,
                schedule,
                command,
                notBefore,
                expirationTime,
                suspend,
                labels,
                null);
            CronReminder reminder = cronJob.ToCronReminder();
            return reminder;
        }

        public async Task<CronReminder?> GetCronReminder(
            string appId,
            string name)
        {
            var key = $"apps/{appId}/CronReminders/{name}";
            CronJob<InvokeHttpRequest>? cronJob =
                await _jobManager.GetCronJob<InvokeHttpRequest>(key);
            return cronJob?.ToCronReminder();
        }

        public async Task<CronReminderItem?> GetCronReminderItem(
            string appId,
            string name,
            DateTime schedule)
        {
            var cronJobKey = $"apps/{appId}/CronReminders/{name}";
            CronJob<InvokeHttpRequest>? cronJob =
                await _jobManager.GetCronJob<InvokeHttpRequest>(cronJobKey);
            if (cronJob is null)
            {
                return null;
            }

            var itemKey = string.Format(FabronConstants.CronItemKeyTemplate, cronJob.Metadata.Uid, schedule.ToString("yyyy-MM-ddTHH:mm:ssZ"));
            Job<InvokeHttpRequest, InvokeHttpRequestResult>? item = await _jobManager.GetJob<InvokeHttpRequest, InvokeHttpRequestResult>(itemKey);
            if (item is null)
            {
                return null;
            }

            return item.ToCronReminderItem();
        }

        public async Task<PaginatedList<CronReminder>> ListCronReminders(
            string appId,
            int pageIndex,
            int pageSize)
        {
            IEnumerable<CronJob<InvokeHttpRequest>>? cronJobs =
                await _jobManager.GetCronJobByLabel<InvokeHttpRequest>(LabelNames.ApplicationId, appId);
            List<CronReminder> list = cronJobs.Select(cronJob => cronJob.ToCronReminder()).ToList();
            return new PaginatedList<CronReminder>(list, list.Count, pageIndex, pageSize);
        }

        public async Task SuspendCronReminder(string appId, string name)
        {
            var key = $"apps/{appId}/CronReminders/{name}";
            await _jobManager.SuspendCronJob(key);
            return;
        }

        public async Task ResumeCronReminder(string appId, string name)
        {
            var key = $"apps/{appId}/CronReminders/{name}";
            await _jobManager.ResumeCronJob(key);
            return;
        }

        public async Task DeleteCronReminder(string appId, string name)
        {
            var key = $"apps/{appId}/CronReminders/{name}";
            await _jobManager.DeleteCronJob(key);
            return;
        }

        public async Task<PaginatedList<CronReminderItem>?> ListCronReminderItems(
            string appId,
            string name,
            int pageIndex,
            int pageSize)
        {
            var key = $"apps/{appId}/CronReminders/{name}";
            IEnumerable<Job<InvokeHttpRequest, InvokeHttpRequestResult>> jobItems
                = await _jobManager.GetJobByCron<InvokeHttpRequest, InvokeHttpRequestResult>(key);
            List<CronReminderItem> list = jobItems.Select(item => item.ToCronReminderItem()).ToList();
            return new PaginatedList<CronReminderItem>(list, list.Count, pageIndex, pageSize);
        }
    }
}
