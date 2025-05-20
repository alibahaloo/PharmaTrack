using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Core.DTOs;
using Schedule.API.Data;

namespace Schedule.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class SchedulesController : ControllerBase
    {
        private readonly ScheduleDBContext _context;
        private readonly IMapper _mapper;


        public SchedulesController(ScheduleDBContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet("daily")]
        public async Task<ActionResult<IEnumerable<ScheduleTaskDto>>> GetDailySchedules (DateTime date)
        {
            // Get the start and end of the specific day
            var startOfDay = date.Date; // Start of the day at 00:00:00
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1); // End of the day at 23:59:59.9999999

            // Fetch tasks that overlap with the day
            var result = await _context.ScheduleTasks
                .Where(st => st.Start <= endOfDay && st.End >= startOfDay)
                .ToListAsync();

            var dtos = _mapper.Map<List<ScheduleTaskDto>>(result);

            return Ok(dtos);
        }

        [HttpGet("daily/user/{userName}")]
        public async Task<ActionResult<IEnumerable<ScheduleTaskDto>>> GetDailySchedulesForUser(DateTime date, string userName)
        {
            // Get the start and end of the specific day
            var startOfDay = date.Date; // Start of the day at 00:00:00
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1); // End of the day at 23:59:59.9999999

            // Fetch tasks that overlap with the day
            var result = await _context.ScheduleTasks
                .Where(st => st.Start <= endOfDay && st.End >= startOfDay && st.UserName == userName)
                .ToListAsync();

            var dtos = _mapper.Map<List<ScheduleTaskDto>>(result);

            return Ok(dtos);
        }

        [HttpGet("weekly")]
        public async Task<ActionResult<IEnumerable<ScheduleTaskDto>>> GetWeeklySchedules(DateTime date)
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

            var dtos = _mapper.Map<List<ScheduleTaskDto>>(result);

            return Ok(dtos);
        }

        [HttpGet("weekly/user/{userName}")]
        public async Task<ActionResult<IEnumerable<ScheduleTaskDto>>    > GetWeeklySchedules(DateTime date, string userName)
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
                    st.End.Date >= startOfWeek &&
                    st.UserName == userName
                )
                .ToListAsync();

            var dtos = _mapper.Map<List<ScheduleTaskDto>>(result);

            return Ok(dtos);
        }

        [HttpGet("monthly")]
        public async Task<ActionResult<IEnumerable<ScheduleTaskDto>>> GetMonthlySchedules(DateTime date)
        {
            // Determine the start and end dates for the given month
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Last day of the month

            // Fetch tasks that overlap with the month
            var result = await _context.ScheduleTasks
                .Where(st => st.Start.Date <= endOfMonth && st.End.Date >= startOfMonth)
                .ToListAsync();

            var dtos = _mapper.Map<List<ScheduleTaskDto>>(result);

            return Ok(dtos);
        }

        [HttpGet("monthly/user/{userName}")]
        public async Task<ActionResult<IEnumerable<ScheduleTaskDto>>> GetMonthlySchedulesForUser(DateTime date, string userName)
        {
            // Determine the start and end dates for the given month
            var startOfMonth = new DateTime(date.Year, date.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1); // Last day of the month

            // Fetch tasks that overlap with the month
            var result = await _context.ScheduleTasks
                .Where(st => st.Start.Date <= endOfMonth && st.End.Date >= startOfMonth && st.UserName == userName)
                .ToListAsync();

            var dtos = _mapper.Map<List<ScheduleTaskDto>>(result);

            return Ok(dtos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScheduleTaskDto>> GetScheduleTask(int id)
        {
            var result = await _context.ScheduleTasks.FindAsync(id);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateScheduleTask(int id, [FromBody] ScheduleTaskDto request)
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
        public async Task<ActionResult<ScheduleTaskDto>> CreateScheduleTask([FromBody] ScheduleTaskDto request)
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
                // 3) map DTO → EF entity
                var entity = _mapper.Map<ScheduleTask>(request);
                /*
                var newScheduleTask = new ScheduleTask
                {
                    Description = request.Description,
                    Start = request.Start,
                    End = request.End,
                    UserName = request.UserName,
                };*/

                _context.ScheduleTasks.Add(entity);
                await _context.SaveChangesAsync();

                // 4) map saved entity → response DTO
                var responseDto = _mapper.Map<ScheduleTaskDto>(entity);

                // 5) return 201 with the new resource
                return CreatedAtAction(
                    nameof(GetDailySchedules),               // your “get” endpoint
                    new { date = responseDto.Start.Date },   // route values
                    responseDto                             // body
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
