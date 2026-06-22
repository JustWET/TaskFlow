namespace TaskFlow.Domain.Entities
{
    public class TaskItem
    {
        public Guid Id { get; set; }
        public Guid TaskListId { get; set; }
        public Guid? CategoryId { get; set; }

        public required string Name { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime? DueDate { get; set; }
        public Priority Priority { get; set; } = Priority.Low;
        public string? Description { get; set; }
        public Category? Category { get; set; }

        public TaskList TaskList { get; set; } = null!;
    }
}
