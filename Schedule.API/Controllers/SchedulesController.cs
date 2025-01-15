using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using Schedule.API.Data;

namespace Schedule.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SchedulesController : ControllerBase
    {
        private readonly ScheduleDBContext _context;

        public SchedulesController(ScheduleDBContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetScheduleTasks(DateTime month)
        {
            var result = await _context.ScheduleTasks.Where(st => st.Start.Date >= month.Date && st.End.Date <= month.Date).ToListAsync();
            return Ok(result);
        }

        [HttpGet("user/{userName}")]
        public async Task<IActionResult> GetUserScheduleTasks(DateTime month, string userName)
        {
            var result = await _context.ScheduleTasks.Where(st => st.Start.Date >= month.Date && st.End.Date <= month.Date && st.UserName == userName).ToListAsync();
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
