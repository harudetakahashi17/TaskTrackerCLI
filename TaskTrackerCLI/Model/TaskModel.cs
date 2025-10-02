namespace TaskTrackerCLI.Model
{
    public class TaskModel
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public TaskTrackerCLI.Enums.TaskStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
