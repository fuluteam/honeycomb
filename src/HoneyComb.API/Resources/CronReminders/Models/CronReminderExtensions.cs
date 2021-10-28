


using Fabron;
using Fabron.Contracts;
using HoneyComb.Commands;

using static HoneyComb.Constants;

namespace HoneyComb.API.Resources.CronReminders.Models
{
    public static class CronReminderExtensions
    {
        public static CronReminder ToCronReminder(this CronJob<InvokeHttpRequest> cronJob)
            => new()
            {
                Name = cronJob.RequireLabel(LabelNames.Name),
                DisplayName = cronJob.GetLabel(FabronConstants.LabelNames.DisplayName),
                AppId = cronJob.RequireLabel(LabelNames.ApplicationId),
                Schedule = cronJob.Spec.Schedule,
                NotBefore = cronJob.Spec.NotBefore,
                ExpirationTime = cronJob.Spec.ExpirationTime,
                Suspend = cronJob.Spec.Suspend,
                Command = cronJob.Spec.CommandData,
                ScheduledJobs = cronJob.Status.Jobs,
                Reason = cronJob.Status.Reason
            };

        public static CronReminderItem ToCronReminderItem(this Job<InvokeHttpRequest, InvokeHttpRequestResult> job)
            => new()
            {
                Command = job.Spec.CommandData,
                Result = job.Status.Result,
                CreatedAt = job.Metadata.CreationTimestamp,
                Schedule = job.Spec.Schedule,
                StartedAt = job.Status.StartedAt,
                FinishedAt = job.Status.FinishedAt,
                Status = job.Status.ExecutionStatus,
                Reason = job.Status.Reason
            };
    }
}
