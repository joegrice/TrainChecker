using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainChecker.Data;
using TrainChecker.Models;
using TrainChecker.Models.DTOs;
using System.Linq;
using System.Threading.Tasks;

namespace TrainChecker.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserPreferencesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserPreferencesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/v1/UserPreferences
        [HttpPost]
        public async Task<ActionResult<UserPreferencesResponse>> PostUserPreferences(UserPreferencesRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {request.UserId} not found.");
            }

            var userPreferences = new UserPreferences
            {
                UserId = request.UserId,
                IsTelegramEnabled = request.IsTelegramEnabled
            };

            _context.UserPreferences.Add(userPreferences);
            await _context.SaveChangesAsync();

            var response = new UserPreferencesResponse
            {
                Id = userPreferences.Id,
                UserId = userPreferences.UserId,
                IsTelegramEnabled = userPreferences.IsTelegramEnabled,
                CreatedAt = userPreferences.CreatedAt,
                UpdatedAt = userPreferences.UpdatedAt
            };

            return CreatedAtAction(nameof(GetUserPreferences), new { id = userPreferences.Id }, response);
        }

        // GET: api/v1/UserPreferences/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserPreferencesResponse>> GetUserPreferences(int id)
        {
            var userPreferences = await _context.UserPreferences
                                                .Include(up => up.User) // Include User if needed for other purposes
                                                .FirstOrDefaultAsync(up => up.Id == id);

            if (userPreferences == null)
            {
                return NotFound();
            }

            var response = new UserPreferencesResponse
            {
                Id = userPreferences.Id,
                UserId = userPreferences.UserId,
                IsTelegramEnabled = userPreferences.IsTelegramEnabled,
                CreatedAt = userPreferences.CreatedAt,
                UpdatedAt = userPreferences.UpdatedAt
            };

            if (userPreferences.IsTelegramEnabled)
            {
                var telegramInfo = await _context.UserTelegramInfo
                                                 .FirstOrDefaultAsync(uti => uti.UserId == userPreferences.UserId);
                if (telegramInfo != null)
                {
                    response.TelegramInfo = new UserTelegramInfoResponse
                    {
                        Id = telegramInfo.Id,
                        UserId = telegramInfo.UserId,
                        ChatId = telegramInfo.ChatId,
                        CreatedAt = telegramInfo.CreatedAt,
                        UpdatedAt = telegramInfo.UpdatedAt
                    };
                }
            }

            return response;
        }

        // GET: api/v1/UserPreferences/email/{userEmail}
        [HttpGet("email/{userEmail}")]
        public async Task<ActionResult<UserPreferencesResponse>> GetUserPreferencesByUserEmail(string userEmail)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound($"User with email {userEmail} not found.");
            }

            var userPreferences = await _context.UserPreferences
                                                .Include(up => up.User)
                                                .FirstOrDefaultAsync(up => up.UserId == user.Id);

            if (userPreferences == null)
            {
                return NotFound($"No preferences found for user with email {userEmail}.");
            }

            var response = new UserPreferencesResponse
            {
                Id = userPreferences.Id,
                UserId = userPreferences.UserId,
                IsTelegramEnabled = userPreferences.IsTelegramEnabled,
                CreatedAt = userPreferences.CreatedAt,
                UpdatedAt = userPreferences.UpdatedAt
            };

            if (userPreferences.IsTelegramEnabled)
            {
                var telegramInfo = await _context.UserTelegramInfo
                                                 .FirstOrDefaultAsync(uti => uti.UserId == userPreferences.UserId);
                if (telegramInfo != null)
                {
                    response.TelegramInfo = new UserTelegramInfoResponse
                    {
                        Id = telegramInfo.Id,
                        UserId = telegramInfo.UserId,
                        ChatId = telegramInfo.ChatId,
                        CreatedAt = telegramInfo.CreatedAt,
                        UpdatedAt = telegramInfo.UpdatedAt
                    };
                }
            }

            return response;
        }

        // GET: api/v1/UserPreferences/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserPreferencesResponse>> GetUserPreferencesByUserId(int userId)
        {
            var userPreferences = await _context.UserPreferences
                                                .Include(up => up.User) // Include User if needed for other purposes
                                                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userPreferences == null)
            {
                return NotFound($"No preferences found for user with ID {userId}.");
            }

            var response = new UserPreferencesResponse
            {
                Id = userPreferences.Id,
                UserId = userPreferences.UserId,
                IsTelegramEnabled = userPreferences.IsTelegramEnabled,
                CreatedAt = userPreferences.CreatedAt,
                UpdatedAt = userPreferences.UpdatedAt
            };

            if (userPreferences.IsTelegramEnabled)
            {
                var telegramInfo = await _context.UserTelegramInfo
                                                 .FirstOrDefaultAsync(uti => uti.UserId == userPreferences.UserId);
                if (telegramInfo != null)
                {
                    response.TelegramInfo = new UserTelegramInfoResponse
                    {
                        Id = telegramInfo.Id,
                        UserId = telegramInfo.UserId,
                        ChatId = telegramInfo.ChatId,
                        CreatedAt = telegramInfo.CreatedAt,
                        UpdatedAt = telegramInfo.UpdatedAt
                    };
                }
            }

            return response;
        }

        // PUT: api/v1/UserPreferences/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserPreferences(int id, UserPreferencesRequest request)
        {
            var userPreferences = await _context.UserPreferences.FindAsync(id);

            if (userPreferences == null)
            {
                return NotFound();
            }

            userPreferences.UserId = request.UserId; // Should ideally not change, but included for completeness
            userPreferences.IsTelegramEnabled = request.IsTelegramEnabled;
            userPreferences.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserPreferencesExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/v1/UserPreferences/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserPreferences(int id)
        {
            var userPreferences = await _context.UserPreferences.FindAsync(id);
            if (userPreferences == null)
            {
                return NotFound();
            }

            _context.UserPreferences.Remove(userPreferences);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserPreferencesExists(int id)
        {
            return _context.UserPreferences.Any(e => e.Id == id);
        }
    }
}