using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TodoListApp.Models;

namespace ToDoList.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [EnableCors("AllowReactApp")]
    public class ToDoListController : ControllerBase
    {
        private readonly TodoDbContext _context;

        public ToDoListController(TodoDbContext context)
        {
            _context = context;
        }

        private string GetUsername()
        {
            return User.Identity?.Name ?? "";
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TodoItem>>> Get()
        {
            var username = GetUsername();
            return await _context.Yapılacaklar
                                 .Where(t => t.UserName == username)
                                 .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> Get(int id)
        {
            var todoItem = await _context.Yapılacaklar.FindAsync(id);
            if (todoItem == null || todoItem.UserName != GetUsername())
            {
                return NotFound();
            }

            return todoItem;
        }

        [HttpPost]
        public async Task<ActionResult<TodoItem>> Post([FromBody] TodoItem todoItem)
        {
            if (string.IsNullOrWhiteSpace(todoItem.Name))
            {
                return BadRequest(new { message = "Görev adı boş olamaz." });
            }

            todoItem.UserName = GetUsername();
            _context.Yapılacaklar.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = todoItem.Id }, todoItem);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TodoItem todoItem)
        {
            if (id != todoItem.Id)
                return BadRequest();

            var existingTodo = await _context.Yapılacaklar.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id && t.UserName == GetUsername());

            if (existingTodo == null)
                return NotFound();

            todoItem.UserName = GetUsername();
            _context.Entry(todoItem).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var todoItem = await _context.Yapılacaklar
                .FirstOrDefaultAsync(t => t.Id == id && t.UserName == GetUsername());

            if (todoItem == null)
                return NotFound();

            _context.Yapılacaklar.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
