using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public enum UserRole
    {
        Admin = 1,
        User = 2
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        public List<TodoItem> ToDoItems { get; set; } = new();

        [Required]
        public UserRole Role { get; set; }
    }
}
