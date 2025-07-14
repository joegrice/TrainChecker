using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrainChecker.Models
{
    public class UserTelegramInfo
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public int? UserPreferencesId { get; set; } 

        [Required]
        public long ChatId { get; set; }

        [MaxLength(256)]
        [Required]
        public string EncryptedBotToken { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey("UserId")]
        public User User { get; set; }

        [ForeignKey("UserPreferencesId")]
        public UserPreferences UserPreferences { get; set; } // Navigation property to UserPreferences
    }
}