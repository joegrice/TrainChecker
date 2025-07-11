using System;

namespace TrainChecker.Models.DTOs
{
    public class UserTelegramInfoResponse
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public long ChatId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}