namespace TaskFlow.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public required string Username { get; set; }
        public required byte[] PasswordHash { get; set; }
        public required byte[] PasswordSalt { get; set; }

        public ICollection<TaskList> TaskLists { get; set; } = new List<TaskList>();
        public ICollection<Category> Categories { get; set; } = new List<Category>();
    }
}
