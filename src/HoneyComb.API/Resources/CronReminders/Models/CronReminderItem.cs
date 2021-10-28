


using System;
using Fabron.Models;
using HoneyComb.Commands;

namespace HoneyComb.API.Resources.CronReminders.Models
{

    /// <summary>
    /// Cron定时器执行记录
    /// </summary>
    public class CronReminderItem
    {
        /// <summary>
        /// 请求命令
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
        /// 错误信息
        /// </summary>
        public string? Reason { get; set; }
    }
}
