using System;
using Fabron;
using Fabron.Contracts;
using Fabron.Models;
using HoneyComb.Commands;
using static HoneyComb.Constants;
namespace HoneyComb.API.Resources.Reminders.Models
{
    /// <summary>
    /// 定时器
    /// </summary>
    public class Reminder
    {
        /// <summary>
        /// 名称，唯一标识，只允许包含 小写字母、数字、短线
        /// </summary>
        public string Name { get; set; } = default!;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// 应用Id
        /// </summary>
        public string AppId { get; set; } = default!;
        /// <summary>
        /// Http请求命令
        /// </summary>
        public InvokeHttpRequest Command { get; set; } = default!;
        /// <summary>
        /// 执行结果
        /// </summary>
        public InvokeHttpRequestResult? Result { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; }
        /// <summary>
        /// 设定执行时间
        /// </summary>
        public DateTime Schedule { get; set; }
        /// <summary>
        /// 开始执行时间
        /// </summary>
        public DateTime? StartedAt { get; set; }
        /// <summary>
        /// 结束执行时间
        /// </summary>
        public DateTime? FinishedAt { get; set; }
        /// <summary>
        /// 执行状态
        /// </summary>
        public ExecutionStatus Status { get; set; }
        /// <summary>
        /// 错误详情
        /// </summary>
        public string? Reason { get; set; }
    }

    public static class ReminderExtensions
    {
        public static Reminder ToReminder(this Job<InvokeHttpRequest, InvokeHttpRequestResult> job)
            => new()
            {
                Name = job.RequireLabel(LabelNames.Name),
                DisplayName = job.GetLabel(FabronConstants.LabelNames.DisplayName),
                AppId = job.RequireLabel(LabelNames.ApplicationId),
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
