using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TodoListApp.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string? Username { get; set; }

        [Required]
        public string? Password { get; set; }

        public List<TodoItem> ToDoItems { get; set; } = new List<TodoItem>();
    }
}
