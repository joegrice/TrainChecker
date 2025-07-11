using System;

namespace TrainChecker.Models.DTOs
{
    public class UserPreferencesResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsTelegramEnabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public UserTelegramInfoResponse? TelegramInfo { get; set; }
    }
}