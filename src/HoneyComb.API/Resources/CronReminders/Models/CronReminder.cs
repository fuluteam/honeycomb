


using System;
using System.Collections.Generic;
using Fabron.Models;
using HoneyComb.Commands;

namespace HoneyComb.API.Resources.CronReminders.Models
{
    /// <summary>
    /// Cron定时器
    /// </summary>
    public class CronReminder
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
        /// Cron表达式
        /// </summary>
        public string Schedule { get; set; } = default!;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? NotBefore { get; set; } = default!;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? ExpirationTime { get; set; } = default!;
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool Suspend { get; set; }
        /// <summary>
        /// 请求命令
        /// </summary>
        public InvokeHttpRequest Command { get; set; } = default!;
        /// <summary>
        /// 最近执行记录
        /// </summary>
        public IEnumerable<JobItem> ScheduledJobs { get; set; } = default!;
        /// <summary>
        /// 错误信息
        /// </summary>
        public string? Reason { get; set; }
    }
}
