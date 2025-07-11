using System.ComponentModel.DataAnnotations;

namespace TrainChecker.Models.DTOs
{
    public class UserTelegramInfoRequest
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public long ChatId { get; set; }

        [MaxLength(256)]
        [Required]
        public string BotToken { get; set; }
    }
}