using TaskFlow.Domain.Entities;

namespace TaskFlow.API.DTOs
{
    public class CreateOrUpdateTaskDto
    {
        public string Name { get; set; } = string.Empty;

        public bool IsCompleted { get; set; }

        public Guid? CategoryId { get; set; }

        public Priority Priority { get; set; }

        public DateTime? DueDate { get; set; }

        public string? Description { get; set; }
    }
}
