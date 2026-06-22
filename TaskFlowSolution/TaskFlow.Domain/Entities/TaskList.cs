
namespace TaskFlow.Domain.Entities
{
    public class TaskList
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        public required string Name { get; set; }

        public User User { get; set; } = null!;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}
