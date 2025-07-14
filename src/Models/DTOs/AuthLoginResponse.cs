namespace TrainChecker.Models.DTOs
{
    public class AuthLoginResponse
    {
        public string Token { get; set; }
        public int UserId { get; set; }
        public string Email { get; set; }
    }
}