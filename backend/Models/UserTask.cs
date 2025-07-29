using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Backend.Models
{
    public class UserTask
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public string Status { get; set; } = "Pending";

        [Required]
        public string Priority { get; set; } = "Normal";

        public DateTime? DueDate { get; set; }

        // Foreign Key
        [ForeignKey("User")]
        public int UserId { get; set; }

        public User? User { get; set; }
    }
}
