

using System.Threading.Tasks;
using HoneyComb.API.Resources.Reminders.Models;
using HoneyComb.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HoneyComb.API.Resources.Apps.Reminders
{
    [ApiController]
    [Route("Apps/{appId}/Reminders", Name = Name)]

    public class Endpoints : ControllerBase
    {
        private const string Name = "Apps.Reminders";
        private const string Name_Register = Name + ".Register";
        private const string Name_Get = Name + ".Get";
        private const string Name_List = Name + ".List";
        private const string Name_Delete = Name + ".Delete";

        private readonly IReminderManager _manager;
        private readonly IReminderQuerier _querier;

        public Endpoints(IReminderManager manager, IReminderQuerier querier)
        {
            _manager = manager;
            _querier = querier;
        }

        [HttpPost(Name = Name_Register)]
        public async Task<IActionResult> Create(string appId, RegisterReminderRequest req)
        {
            Reminder? reminder = await _manager.RegisterReminder(
                appId,
                req.Name,
                req.DisplayName,
                req.Schedule,
                req.Command);
            return CreatedAtRoute(Name_Get, new { appId, name = reminder.Name }, reminder);
        }

        [HttpGet("{name}", Name = Name_Get)]
        public async Task<IActionResult> Get(string appId, string name)
        {
            Reminder? reminder = await _manager.GetReminder(appId, name);
            return reminder is null ? NotFound() : Ok(reminder);
        }

        [HttpGet(Name = Name_List)]
        public async Task<ActionResult<PaginatedList<Reminder>>> List(string appId, string? search, int pageIndex = 1, int pageSize = 20)
        {
            PaginatedList<Reminder> reminders;
            reminders = await _querier.ListRemindersByDisplayNameAsync(appId, search, (pageIndex - 1) * pageSize, pageSize);
            return Ok(reminders);
        }

        [HttpDelete("{name}", Name = Name_Delete)]
        public async Task<IActionResult> Delete(string appId, string name)
        {
            await _manager.DeleteReminder(appId, name);
            return Ok();
        }

    }
}
