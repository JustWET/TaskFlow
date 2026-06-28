using TaskFlow.Domain.Entities;

namespace TaskFlow.API.DTOs
{
    public class ResponseTaskDto
    {
        public Guid Id { get; set; }
        public Guid TaskListId { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public Guid? CategoryId { get; set; }

        public Priority Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Description { get; set; }
    }
}
