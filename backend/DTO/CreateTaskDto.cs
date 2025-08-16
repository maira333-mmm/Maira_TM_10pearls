namespace Backend.DTO
{
    public class CreateTaskDto
    {
        public string Title { get; set; } = default!;       // Required field
        public string Description { get; set; } = default!; // Required field
        public DateTime DueDate { get; set; }               // Struct -> no null issue
        public string Status { get; set; } = default!;      // Required field
        public string Priority { get; set; } = default!;    // Required field
        public int UserId { get; set; }                     // int -> no null issue
    }
}
