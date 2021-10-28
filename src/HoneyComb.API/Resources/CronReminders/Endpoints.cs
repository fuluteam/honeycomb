


using System;
using System.Threading.Tasks;
using HoneyComb.API.Extensions;
using HoneyComb.API.Resources.CronReminders.Models;
using HoneyComb.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HoneyComb.API.Resources.CronReminders
{
    [ApiController]
    [Route("CronReminders", Name = Name)]

    public class Endpoints : ControllerBase
    {
        private const string Name = "CronReminders";
        private const string Name_Register = Name + ".Register";
        private const string Name_Get = Name + ".Get";
        private const string Name_GetItems = Name + ".GetItems";
        private const string Name_GetItem = Name + ".GetItem";
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

        [HttpPost(Name = Name_Register)]
        public async Task<IActionResult> Register(RegisterCronReminderRequest req)
        {
            var appId = HttpContext.GetAppId();
            CronReminder? reminder = await _manager.RegisterCronReminder(
                appId,
                req.Name,
                req.DisplayName,
                req.Schedule,
                req.Command,
                req.NotBefore,
                req.ExpirationTime,
                req.Suspend);
            return CreatedAtRoute(Name_Get, new { name = reminder.Name }, reminder);
        }

        [HttpGet("{name}", Name = Name_Get)]
        public async Task<IActionResult> Get(string name)
        {
            var appId = HttpContext.GetAppId();
            CronReminder? reminder = await _manager.GetCronReminder(appId, name);
            return reminder is null ? NotFound() : Ok(reminder);
        }

        [HttpGet("{name}/items", Name = Name_GetItems)]
        public async Task<IActionResult> GetItems(string name, int pageIndex = 1, int pageSize = 20)
        {
            var appId = HttpContext.GetAppId();
            var key = $"apps/{appId}/CronReminders/{name}";
            PaginatedList<CronReminderItem>? items = await _querier.ListCronReminderItemsAsync(key, pageIndex, pageSize);
            return items is null ? NotFound() : Ok(items);
        }

        [HttpGet("{name}/items/{schedule}", Name = Name_GetItem)]
        public async Task<IActionResult> GetItem(string name, DateTime schedule)
        {
            var appId = HttpContext.GetAppId();
            CronReminderItem? item = await _manager.GetCronReminderItem(appId, name, schedule);
            return item is null ? NotFound() : Ok(item);
        }

        [HttpDelete("{name}", Name = Name_Delete)]
        public async Task<IActionResult> Delete(string name)
        {
            var appId = HttpContext.GetAppId();
            await _manager.DeleteCronReminder(appId, name);
            return NoContent();
        }

        [HttpPost("{name}/suspension", Name = Name_Suspend)]
        public async Task<IActionResult> Suspend(string name)
        {
            var appId = HttpContext.GetAppId();
            await _manager.SuspendCronReminder(appId, name);
            return Ok();
        }

        [HttpDelete("{name}/suspension", Name = Name_Resume)]
        public async Task<IActionResult> Resume(string name)
        {
            var appId = HttpContext.GetAppId();
            await _manager.ResumeCronReminder(appId, name);
            return NoContent();
        }

    }
}
