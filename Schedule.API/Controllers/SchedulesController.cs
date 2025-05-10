using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using Schedule.API.Data;
using System.Security.Claims;

namespace Schedule.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly ScheduleDBContext _context;

        public SchedulesController(ScheduleDBContext context)
        {
            _context = context;
        }

        [HttpGet("daily")]
        public async Task<IActionResult> GetDailySchedules (DateTime date)
        {
            // Get the start and end of the specific day
            var startOfDay = date.Date; // Start of the day at 00:00:00
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1); // End of the day at 23:59:59.9999999

            // Fetch tasks that overlap with the day
            var result = await _context.ScheduleTasks
                .Where(st => st.Start <= endOfDay && st.End >= startOfDay)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("daily/user/{userName}")]
        public async Task<IActionResult> GetDailySchedulesForUser(DateTime date, string userName)
        {
            // Get the start and end of the specific day
            var startOfDay = date.Date; // Start of the day at 00:00:00
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1); // End of the day at 23:59:59.9999999

            // Fetch tasks that overlap with the day
            var result = await _context.ScheduleTasks
                .Where(st => st.Start <= endOfDay && st.End >= startOfDay && st.UserName == userName)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("weekly")]
        public async Task<IActionResult> GetWeeklySchedules(DateTime date)
        {
            // 1) Find the Monday of the week that 'date' falls in
            int diff = (7 + ((int)date.DayOfWeek - (int)DayOfWeek.Monday)) % 7;
            var startOfWeek = date.Date.AddDays(-diff);

            // 2) End of week is Sunday
            var endOfWeek = startOfWeek.AddDays(6);

            // 3) Fetch tasks overlapping that range
            var result = await _context.ScheduleTasks
                .Where(st =>
                    st.Start.Date <= endOfWeek &&
                    st.End.Date >= startOfWeek
                )
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("monthly")]
        public async Task<IActionResult> GetMonthlySchedules(DateTime date)
        {
            // Determine the start and end dates for the given month
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Last day of the month

            // Fetch tasks that overlap with the month
            var result = await _context.ScheduleTasks
                .Where(st => st.Start.Date <= endOfMonth && st.End.Date >= startOfMonth)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("monthly/user/{userName}")]
        public async Task<IActionResult> GetMonthlySchedulesForUser(DateTime date, string userName)
        {
            // Determine the start and end dates for the given month
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Last day of the month

            // Fetch tasks that overlap with the month
            var result = await _context.ScheduleTasks
                .Where(st => st.Start.Date <= endOfMonth && st.End.Date >= startOfMonth && st.UserName == userName)
                .ToListAsync();

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetScheduleTask(int id)
        {
            var result = await _context.ScheduleTasks.FindAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScheduleTask(int id, [FromBody] ScheduleTaskRequest request)
        {
            var existingScheduleTask = await _context.ScheduleTasks.FindAsync(id);

            if (existingScheduleTask == null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // Check if the Start and End dates are in the past
            if (request.Start.Date < DateTime.Today || request.End.Date < DateTime.Today)
            {
                return BadRequest("Start and End dates must not be in the past.");
            }

            try
            {
                existingScheduleTask.Start =request.Start;
                existingScheduleTask.End =request.End;
                existingScheduleTask.UserName =request.UserName;
                existingScheduleTask.Description =request.Description;

                // Save changes to the database
                _context.ScheduleTasks.Update(existingScheduleTask);
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScheduleTask(int id)
        {
            // Find the product by id
            var scheduledTask = await _context.ScheduleTasks.FindAsync(id);

            if (scheduledTask == null)
            {
                return NotFound();
            }

            try
            {
                // Remove the product from the database
                _context.ScheduleTasks.Remove(scheduledTask);

                // Save changes
                await _context.SaveChangesAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during deletion
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateScheduleTask([FromBody] ScheduleTaskRequest request)
        {
            //Check if the user is actually an admin
            //var isAdmin = User.IsInRole("admin");
            // Check if the Start and End dates are in the past
            if (request.Start.Date < DateTime.Today || request.End.Date < DateTime.Today)
            {
                return BadRequest("Start and End dates must not be in the past.");
            }

            try
            {
                var newScheduleTask = new ScheduleTask
                {
                    Description = request.Description,
                    Start = request.Start,
                    End = request.End,
                    UserName = request.UserName,
                };

                _context.ScheduleTasks.Add(newScheduleTask);
                await _context.SaveChangesAsync();

                return Ok(newScheduleTask);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
