using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrainChecker.Data;
using TrainChecker.Models;
using TrainChecker.Models.DTOs;
using TrainChecker.Services.Security;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TrainChecker.Controllers.v1
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class UserTelegramInfoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UserTelegramInfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // POST: api/v1/UserTelegramInfo
        [HttpPost]
        public async Task<ActionResult<UserTelegramInfoResponse>> PostUserTelegramInfo(UserTelegramInfoRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
            {
                return NotFound($"User with ID {request.UserId} not found.");
            }

            var userTelegramInfo = new UserTelegramInfo
            {
                UserId = request.UserId,
                ChatId = request.ChatId,
                EncryptedBotToken = PasswordHasher.HashPassword(request.BotToken)
            };

            _context.UserTelegramInfo.Add(userTelegramInfo);
            await _context.SaveChangesAsync();

            var response = new UserTelegramInfoResponse
            {
                Id = userTelegramInfo.Id,
                UserId = userTelegramInfo.UserId,
                ChatId = userTelegramInfo.ChatId,
                CreatedAt = userTelegramInfo.CreatedAt,
                UpdatedAt = userTelegramInfo.UpdatedAt
            };

            return CreatedAtAction(nameof(GetUserTelegramInfo), new { id = userTelegramInfo.Id }, response);
        }

        // GET: api/v1/UserTelegramInfo/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<UserTelegramInfoResponse>> GetUserTelegramInfo(int id)
        {
            var userTelegramInfo = await _context.UserTelegramInfo.FindAsync(id);

            if (userTelegramInfo == null)
            {
                return NotFound();
            }

            var response = new UserTelegramInfoResponse
            {
                Id = userTelegramInfo.Id,
                UserId = userTelegramInfo.UserId,
                ChatId = userTelegramInfo.ChatId,
                CreatedAt = userTelegramInfo.CreatedAt,
                UpdatedAt = userTelegramInfo.UpdatedAt
            };

            return response;
        }

        // GET: api/v1/UserTelegramInfo/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<UserTelegramInfoResponse>>> GetUserTelegramInfoByUserId(int userId)
        {
            var userTelegramInfoList = await _context.UserTelegramInfo
                                                    .Where(uti => uti.UserId == userId)
                                                    .ToListAsync();

            if (!userTelegramInfoList.Any())
            {
                return NotFound($"No Telegram info found for user with ID {userId}.");
            }

            var responseList = userTelegramInfoList.Select(uti => new UserTelegramInfoResponse
            {
                Id = uti.Id,
                UserId = uti.UserId,
                ChatId = uti.ChatId,
                CreatedAt = uti.CreatedAt,
                UpdatedAt = uti.UpdatedAt
            }).ToList();

            return responseList;
        }

        // PUT: api/v1/UserTelegramInfo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserTelegramInfo(int id, UserTelegramInfoRequest request)
        {
            var userTelegramInfo = await _context.UserTelegramInfo.FindAsync(id);

            if (userTelegramInfo == null)
            {
                return NotFound();
            }

            // Optionally verify password if needed for update, or just update if it's a full replacement
            // For this example, we'll assume the request.Password is the new password to hash
            userTelegramInfo.UserId = request.UserId; // Should ideally not change, but included for completeness
            userTelegramInfo.ChatId = request.ChatId;
            userTelegramInfo.EncryptedBotToken = PasswordHasher.HashPassword(request.BotToken);
            userTelegramInfo.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserTelegramInfoExists(id))
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

        // DELETE: api/v1/UserTelegramInfo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserTelegramInfo(int id)
        {
            var userTelegramInfo = await _context.UserTelegramInfo.FindAsync(id);
            if (userTelegramInfo == null)
            {
                return NotFound();
            }

            _context.UserTelegramInfo.Remove(userTelegramInfo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserTelegramInfoExists(int id)
        {
            return _context.UserTelegramInfo.Any(e => e.Id == id);
        }
    }
}