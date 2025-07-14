using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainChecker.Data;
using TrainChecker.Models;
using TrainChecker.Models.DTOs;
using TrainChecker.Services.Security;
using TrainChecker.Services.Telegram;
using System.Linq;
using System.Threading.Tasks;

namespace TrainChecker.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserPreferencesController(ApplicationDbContext context) : ControllerBase
    {
        // POST: api/v1/UserPreferences
        [HttpPost]
        public async Task<ActionResult<UserPreferencesResponse>> PostUserPreferences(UserPreferencesRequest request)
        {
            var user = await context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {request.UserId} not found.");
            }

            var userPreferences = new UserPreferences
            {
                UserId = request.UserId,
                IsTelegramEnabled = request.IsTelegramEnabled
            };

            context.UserPreferences.Add(userPreferences);
            await context.SaveChangesAsync();

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
            var userPreferences = await context.UserPreferences
                                                 .Include(up => up.User)
                                                 .Include(up => up.UserTelegramInfo) // Include UserTelegramInfo
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

            if (userPreferences.IsTelegramEnabled && userPreferences.UserTelegramInfo != null)
            {
                response.TelegramInfo = new UserTelegramInfoResponse
                {
                    Id = userPreferences.UserTelegramInfo.Id,
                    UserId = userPreferences.UserTelegramInfo.UserId,
                    ChatId = userPreferences.UserTelegramInfo.ChatId,
                    CreatedAt = userPreferences.UserTelegramInfo.CreatedAt,
                    UpdatedAt = userPreferences.UserTelegramInfo.UpdatedAt
                };
            }

            return response;
        }

        // GET: api/v1/UserPreferences/email/{userEmail}
        [HttpGet("email/{userEmail}")]
        public async Task<ActionResult<UserPreferencesResponse>> GetUserPreferencesByUserEmail(string userEmail)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (user == null)
            {
                return NotFound($"User with email {userEmail} not found.");
            }

            var userPreferences = await context.UserPreferences
                                                 .Include(up => up.User)
                                                 .Include(up => up.UserTelegramInfo) // Include UserTelegramInfo
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

            if (userPreferences.IsTelegramEnabled && userPreferences.UserTelegramInfo != null)
            {
                response.TelegramInfo = new UserTelegramInfoResponse
                {
                    Id = userPreferences.UserTelegramInfo.Id,
                    UserId = userPreferences.UserTelegramInfo.UserId,
                    ChatId = userPreferences.UserTelegramInfo.ChatId,
                    CreatedAt = userPreferences.UserTelegramInfo.CreatedAt,
                    UpdatedAt = userPreferences.UserTelegramInfo.UpdatedAt
                };
            }

            return response;
        }

        // GET: api/v1/UserPreferences/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<UserPreferencesResponse>> GetUserPreferencesByUserId(int userId)
        {
            var userPreferences = await context.UserPreferences
                .Include(up => up.User)
                .Include(up => up.UserTelegramInfo)
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

            if (userPreferences is { IsTelegramEnabled: true, UserTelegramInfo: not null })
            {
                Console.WriteLine($"GetUserPreferencesByUserId: UserTelegramInfo found. ChatId={userPreferences.UserTelegramInfo.ChatId}");
                response.TelegramInfo = new UserTelegramInfoResponse
                {
                    Id = userPreferences.UserTelegramInfo.Id,
                    UserId = userPreferences.UserTelegramInfo.UserId,
                    ChatId = userPreferences.UserTelegramInfo.ChatId,
                    CreatedAt = userPreferences.UserTelegramInfo.CreatedAt,
                    UpdatedAt = userPreferences.UserTelegramInfo.UpdatedAt
                };
            } else {
                Console.WriteLine("GetUserPreferencesByUserId: UserTelegramInfo is null or notifications are disabled.");
            }

            return response;
        }

        // PUT: api/v1/UserPreferences/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutUserPreferences(int id, UserPreferencesRequest request)
        {
            var userPreferences = await context.UserPreferences.FindAsync(id);

            if (userPreferences == null)
            {
                return NotFound();
            }

            userPreferences.UserId = request.UserId; // Should ideally not change, but included for completeness
            userPreferences.IsTelegramEnabled = request.IsTelegramEnabled;
            userPreferences.UpdatedAt = DateTime.UtcNow;

            try
            {
                await context.SaveChangesAsync();
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

        // PUT: api/v1/UserPreferences/user/{userId} (Upsert)
        [HttpPut("user/{userId:int}")]
        public async Task<IActionResult> UpsertUserPreferences(int userId, UserPreferencesRequest request)
        {
            Console.WriteLine($"UpsertUserPreferences: UserId={userId}, IsTelegramEnabled={request.IsTelegramEnabled}, ChatId={request.ChatId}, BotToken={request.BotToken}");

            var userPreferences = await context.UserPreferences.FirstOrDefaultAsync(up => up.UserId == userId);

            if (userPreferences == null)
            {
                Console.WriteLine("UpsertUserPreferences: UserPreferences not found, creating new.");
                // Create new preferences if not found
                var user = await context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound($"User with ID {userId} not found.");
                }

                userPreferences = new UserPreferences
                {
                    UserId = userId,
                    IsTelegramEnabled = request.IsTelegramEnabled,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                context.UserPreferences.Add(userPreferences);
            }
            else
            {
                Console.WriteLine("UpsertUserPreferences: UserPreferences found, updating existing.");
                // Update existing preferences
                userPreferences.IsTelegramEnabled = request.IsTelegramEnabled;
                userPreferences.UpdatedAt = DateTime.UtcNow;
                context.UserPreferences.Update(userPreferences);
            }

            await context.SaveChangesAsync();
            Console.WriteLine("UpsertUserPreferences: UserPreferences saved.");

            // Handle UserTelegramInfo
            if (request.IsTelegramEnabled)
            {
                var userTelegramInfo = await context.UserTelegramInfo.FirstOrDefaultAsync(uti => uti.UserId == userId);
                if (userTelegramInfo == null)
                {
                    Console.WriteLine("UpsertUserPreferences: UserTelegramInfo not found, creating new.");
                    userTelegramInfo = new UserTelegramInfo
                    {
                        UserId = userId,
                        ChatId = request.ChatId,
                        EncryptedBotToken = PasswordHasher.HashPassword(request.BotToken),
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    context.UserTelegramInfo.Add(userTelegramInfo);
                }
                else
                {
                    Console.WriteLine("UpsertUserPreferences: UserTelegramInfo found, updating existing.");
                    userTelegramInfo.ChatId = request.ChatId;
                    userTelegramInfo.EncryptedBotToken = PasswordHasher.HashPassword(request.BotToken);
                    userTelegramInfo.UpdatedAt = DateTime.UtcNow;
                    context.UserTelegramInfo.Update(userTelegramInfo);
                }
                await context.SaveChangesAsync();
                Console.WriteLine("UpsertUserPreferences: UserTelegramInfo saved.");
            }
            else // If notifications are disabled, remove Telegram info
            {
                var userTelegramInfo = await context.UserTelegramInfo.FirstOrDefaultAsync(uti => uti.UserId == userId);
                if (userTelegramInfo != null)
                {
                    Console.WriteLine("UpsertUserPreferences: Notifications disabled, removing UserTelegramInfo.");
                    context.UserTelegramInfo.Remove(userTelegramInfo);
                    await context.SaveChangesAsync();
                    Console.WriteLine("UpsertUserPreferences: UserTelegramInfo removed.");
                }
            }

            return NoContent();
        }

        // DELETE: api/v1/UserPreferences/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserPreferences(int id)
        {
            var userPreferences = await context.UserPreferences.FindAsync(id);
            if (userPreferences == null)
            {
                return NotFound();
            }

            context.UserPreferences.Remove(userPreferences);
            await context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserPreferencesExists(int id)
        {
            return context.UserPreferences.Any(e => e.Id == id);
        }

        [HttpPost("validate-telegram")]
        public async Task<ActionResult<bool>> ValidateTelegram(UserTelegramInfoRequest request)
        {
            var telegramValidationService = HttpContext.RequestServices.GetRequiredService<ITelegramValidationService>();
            var isValid = await telegramValidationService.ValidateAndSendMessageAsync(request.BotToken, request.ChatId, "This is a test message from TrainChecker.");
            return Ok(isValid);
        }
    }
}