


using System;
using System.ComponentModel.DataAnnotations;
using HoneyComb.Commands;
namespace HoneyComb.API.Resources.Reminders.Models
{

    /// <summary>
    /// 创建或更新定时器请求
    /// </summary>
    public class RegisterReminderRequest
    {
        /// <summary>
        /// 定时器名，唯一标识，只允许包含 小写字母、数字、短线
        /// </summary>
        public string Name { get; set; } = default!;
        /// <summary>
        /// 显示名称
        /// </summary>
        public string? DisplayName { get; set; }
        /// <summary>
        /// 设定执行时间
        /// </summary>
        [Required]
        public DateTime Schedule { get; set; } = default!;
        /// <summary>
        /// Http请求命令
        /// </summary>
        public InvokeHttpRequest Command { get; set; } = default!;
    }

}
