


using System;
using HoneyComb.Commands;

namespace HoneyComb.API.Resources.CronReminders.Models
{
    /// <summary>
    /// 注册或更新Cron定时器请求
    /// </summary>
    public class RegisterCronReminderRequest
    {
        /// <summary>
        /// 名称，唯一标识，只允许包含 小写字母、数字、短线
        /// </summary>
        [Name]
        public string Name { get; set; } = default!;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// Cron表达式
        /// </summary>
        [Cron]
        public string Schedule { get; set; } = default!;
        /// <summary>
        /// 开始时间
        /// </summary>
        public DateTime? NotBefore { get; set; }
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime? ExpirationTime { get; set; }
        /// <summary>
        /// 是否暂停
        /// </summary>
        public bool Suspend { get; set; }
        /// <summary>
        /// 请求命令
        /// </summary>
        public InvokeHttpRequest Command { get; set; } = default!;
    }
}
