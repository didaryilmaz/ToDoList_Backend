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

        [HttpGet("mytodos")]
        public async Task<ActionResult<IEnumerable<TodoItem>>> GetMyTodos()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "userId");
            if (userIdClaim == null)
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdClaim.Value);

            var myTodos = await _context.Yapılacaklar
                .Where(t => t.UserId == userId)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(myTodos);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<TodoItem>> Get(int id)
        {
            var todoItem = await _context.Yapılacaklar.FindAsync(id);
            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TodoItem todoItem)
        {
            if (string.IsNullOrWhiteSpace(todoItem.Name))
            {
                return BadRequest(new { message = "Görev adı boş olamaz." });
            }

            var user = await _context.Users.FindAsync(todoItem.UserId);
            if (user == null)
            {
                return BadRequest("Geçersiz kullanıcı ID'si.");
            }

            todoItem.CreatedAt = DateTime.UtcNow;

            _context.Yapılacaklar.Add(todoItem);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(Get), new { id = todoItem.Id }, todoItem);
        }




        [HttpPut("mytodos/{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] TodoItem todoItem)
        {
            if (id != todoItem.Id)
                return BadRequest();

            var existingTodo = await _context.Yapılacaklar.AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (existingTodo == null)
                return NotFound();

            _context.Entry(todoItem).State = EntityState.Modified;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("mytodos/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var todoItem = await _context.Yapılacaklar.FindAsync(id);

            if (todoItem == null)
                return NotFound();

            _context.Yapılacaklar.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllTodos([FromQuery] string title = "", [FromQuery] bool? completed = null, [FromQuery] int page = 0, [FromQuery] int size = 20)
        {
            var query = _context.Yapılacaklar.AsQueryable();

            if (!string.IsNullOrWhiteSpace(title))
            {
                query = query.Where(t => t.Name.Contains(title));
            }

            if (completed.HasValue)
            {
                query = query.Where(t => t.IsCompleted == completed.Value);
            }

            var totalCount = await query.CountAsync();

            var todos = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip(page * size)
                .Take(size)
                .ToListAsync();

            return Ok(new
            {
                totalCount,
                todos
            });
        }


    }
}