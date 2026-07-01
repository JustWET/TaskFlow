namespace TaskFlow.Domain.Models
{
    public class TaskQuery
    {
        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 20;

        public string? Search { get; set; }

        public Guid? CategoryId { get; set; }

        public TaskSortBy? SortBy { get; set; }

        public bool Descending { get; set; }
    }
}
