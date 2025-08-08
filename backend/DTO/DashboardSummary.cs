namespace Backend.DTO
{
    public class DashboardSummary
    {
        public int TotalUsers { get; set; }
        public int TotalTasks { get; set; }
        public int CompletedTasks { get; set; }
        public int PendingTasks { get; set; }
    }
}
