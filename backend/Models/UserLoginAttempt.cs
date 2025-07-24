using System;

namespace Backend.Models
{
    public class UserLoginAttempt
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public bool IsSuccessful { get; set; }
        public DateTime AttemptTime { get; set; } = DateTime.UtcNow;
        public string? IPAddress { get; set; }
        public string? UserAgent { get; set; }

        public User? User { get; set; }
    }
}
