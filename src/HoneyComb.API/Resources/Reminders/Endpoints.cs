


using System.Threading.Tasks;
using HoneyComb.API.Extensions;
using HoneyComb.API.Resources.Reminders.Models;
using HoneyComb.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace HoneyComb.API.Resources.Reminders
{
    [ApiController]
    [Route("Reminders", Name = Name)]

    public class Endpoints : ControllerBase
    {
        private const string Name = "Reminders";
        private const string Name_Register = Name + ".Register";
        private const string Name_Get = Name + ".Get";
        private const string Name_Delete = Name + ".Delete";

        private readonly IReminderManager _manager;

        public Endpoints(IReminderManager manager) => _manager = manager;

        [HttpPost(Name = Name_Register)]
        public async Task<IActionResult> Register(RegisterReminderRequest req)
        {
            var appId = HttpContext.GetAppId();
            Reminder? reminder = await _manager.RegisterReminder(
                appId,
                req.Name,
                req.DisplayName,
                req.Schedule,
                req.Command);
            return CreatedAtRoute(Name_Get, new { name = reminder.Name }, reminder);
        }

        [HttpGet("{name}", Name = Name_Get)]
        public async Task<IActionResult> Get(string name)
        {
            var appId = HttpContext.GetAppId();
            Reminder? reminder = await _manager.GetReminder(appId, name);
            return reminder is null ? NotFound() : Ok(reminder);
        }


        [HttpDelete("{name}", Name = Name_Delete)]
        public async Task<IActionResult> Delete(string name)
        {
            var appId = HttpContext.GetAppId();
            await _manager.DeleteReminder(appId, name);
            return Ok();
        }

    }
}
