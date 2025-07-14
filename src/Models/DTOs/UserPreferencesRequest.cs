using System.ComponentModel.DataAnnotations;

namespace TrainChecker.Models.DTOs
{
    public class UserPreferencesRequest
    {
        [Required]
        public int UserId { get; set; }

        public bool IsTelegramEnabled { get; set; }

        public long ChatId { get; set; }
        public string? BotToken { get; set; }
    }
}