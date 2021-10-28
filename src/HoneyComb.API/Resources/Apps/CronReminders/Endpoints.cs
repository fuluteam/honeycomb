


using System;
using System.Threading.Tasks;
using HoneyComb.API.Resources.CronReminders.Models;
using HoneyComb.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HoneyComb.API.Resources.Apps.CronReminders
{
    [Produces("application/json")]
    [ApiController]
    [Route("Apps/{appId}/CronReminders", Name = Name)]

    public class Endpoints : ControllerBase
    {
        private const string Name = "Apps.CronReminders";
        private const string Name_Register = Name + ".Register";
        private const string Name_Get = Name + ".Get";
        private const string Name_GetItems = Name + ".GetItems";
        private const string Name_GetItem = Name + ".GetItem";
        private const string Name_List = Name + ".List";
        // private const string Name_Search = Name + ".Search";
        private const string Name_Delete = Name + ".Delete";
        private const string Name_Suspend = Name + ".Suspend";
        private const string Name_Resume = Name + ".Resume";

        private readonly IReminderManager _manager;
        private readonly IReminderQuerier _querier;

        public Endpoints(IReminderManager manager, IReminderQuerier querier)
        {
            _manager = manager;
            _querier = querier;
        }

        /// <summary>
        /// 注册或更新CronReminder
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="req"></param>
        /// <returns>已创建的CronReminder</returns>
        [HttpPost(Name = Name_Register)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CronReminder>> Create(string appId, RegisterCronReminderRequest req)
        {
            CronReminder? reminder = await _manager.RegisterCronReminder(
                appId,
                req.Name,
                req.DisplayName,
                req.Schedule,
                req.Command,
                req.NotBefore,
                req.ExpirationTime,
                req.Suspend);
            return CreatedAtRoute(Name_Get, new { appId = appId, name = reminder.Name }, reminder);
        }

        /// <summary>
        /// 获取CronReminder详情
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="name">CronReminder名称</param>
        /// <returns>CronReminder</returns>
        [HttpGet("{name}", Name = Name_Get)]
        public async Task<ActionResult<CronReminder>> Get(string appId, string name)
        {
            CronReminder? reminder = await _manager.GetCronReminder(appId, name);
            return reminder is null ? NotFound() : Ok(reminder);
        }

        /// <summary>
        /// 获取CronReminder记录列表
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="name">CronReminder名称</param>
        /// <param name="pageIndex">页码</param>
        /// <param name="pageSize">分页大小</param>
        /// <returns></returns>
        [HttpGet("{name}/items", Name = Name_GetItems)]
        public async Task<ActionResult<PaginatedList<CronReminderItem>>> GetItems(string appId, string name, int pageIndex = 1, int pageSize = 20)
        {
            var key = $"apps/{appId}/CronReminders/{name}";
            PaginatedList<CronReminderItem>? items = await _querier.ListCronReminderItemsAsync(key, (pageIndex - 1) * pageSize, pageSize);
            return items is null ? NotFound() : Ok(items);
        }


        /// <summary>
        /// 获取CronReminder记录
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="name">CronReminder名称</param>
        /// <param name="schedule">调度时间戳</param>
        [HttpGet("{name}/items/{schedule}", Name = Name_GetItem)]
        public async Task<IActionResult> GetItem(string appId, string name, DateTime schedule)
        {
            CronReminderItem? item = await _manager.GetCronReminderItem(appId, name, schedule);
            return item is null ? NotFound() : Ok(item);
        }

        /// <summary>
        /// 列出CronReminder
        /// </summary>
        /// <param name="appId">应用Id</param>
        /// <param name="search">根据显示名称过滤</param>
        /// <param name="pageIndex">当前页</param>
        /// <param name="pageSize">页容量</param>
        /// <returns></returns>
        [HttpGet(Name = Name_List)]
        public async Task<ActionResult<PaginatedList<CronReminder>>> List(string appId, string? search, int pageIndex = 1, int pageSize = 20)
        {
            PaginatedList<CronReminder> cronReminders;
            cronReminders = await _querier.ListCronReminderByDisplayNameAsync(appId, search, (pageIndex - 1) * pageSize, pageSize);
            return Ok(cronReminders);
        }


        [HttpDelete("{name}", Name = Name_Delete)]
        public async Task<IActionResult> Delete(string appId, string name)
        {
            await _manager.DeleteCronReminder(appId, name);
            return NoContent();
        }

        /// <summary>
        /// 暂停CronReminder
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpPost("{name}/suspension", Name = Name_Suspend)]
        public async Task<IActionResult> Suspend(string appId, string name)
        {
            await _manager.SuspendCronReminder(appId, name);
            return Ok();
        }

        /// <summary>
        /// 恢复CronReminder
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        [HttpDelete("{name}/suspension", Name = Name_Resume)]
        public async Task<IActionResult> Resume(string appId, string name)
        {
            await _manager.ResumeCronReminder(appId, name);
            return NoContent();
        }

    }
}
