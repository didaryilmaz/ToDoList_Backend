using TodoListApp.Models;

public class TodoItem
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Name { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsCompleted { get; set; }

    public int UserId { get; set; }
    public User? User { get; set; }
}
