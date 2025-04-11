using System;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class TodoItem
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(255)]
        public string? Name { get; set; }  
        public bool IsCompleted { get; set; }  
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int UserId { get; set; }  // Foreign key for the User table
    }
}
