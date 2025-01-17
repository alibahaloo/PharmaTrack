using Auth.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaTrack.Shared.APIModels;
using PharmaTrack.Shared.DBModels;
using PharmaTrack.Shared.Services;

namespace Auth.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AuthDBContext _context;

        public UsersController(AuthDBContext context)
        {
            _context = context;
        }

        [HttpGet("usernames")]
        public async Task<IActionResult> GetUsernames()
        {
            var usernames = await _context.ApplicationUsers
                                  .Select(u => u.UserName)
                                  .ToListAsync();
            return Ok(usernames);
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(int curPage = 1)
        {
            IQueryable<ApplicationUser> query = _context.ApplicationUsers;

            // Project the fields you want to include
            var projectedQuery = query.Select(user => new UserDto
            {
                Id = user.Id,
                UserName = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty
            });

            var result = await EFExtensions.GetPaged(projectedQuery, curPage);

            var response = new PagedResponse<UserDto>
            {
                Data = [.. result.Data],
                CurrentPage = curPage,
                CurrentItemCount = result.Data.Count,
                TotalPageCount = result.PageCount,
                TotalItemCount = result.RowCount
            };

            return Ok(response);
        }
    }
}
