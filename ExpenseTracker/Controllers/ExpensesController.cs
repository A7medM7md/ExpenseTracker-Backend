using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models.Entities;
using ExpenseTracker.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ExpenseTracker.Dtos;

namespace ExpenseTracker.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ExpensesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetExpenses()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var expenses = _context.Expenses
                .Where(e => e.UserId == userId)
                .Select(e => new ExpenseDto
                {
                    Id = e.Id,
                    Amount = e.Amount,
                    Currency = e.Currency,
                    Category = e.Category,
                    Date = e.Date,
                    Description = e.Description
                }).ToList();

            return Ok(expenses);
        }


        [HttpPost]
        public IActionResult AddExpense([FromBody] CreateExpenseDto expenseDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var expense = new Expense
            {
                UserId = userId,
                Amount = expenseDto.Amount,
                Currency = expenseDto.Currency,
                Category = expenseDto.Category,
                Date = expenseDto.Date,
                Description = expenseDto.Description
            };

            _context.Expenses.Add(expense);
            _context.SaveChanges();

            return Ok(new ExpenseDto
            {
                Id = expense.Id,
                Amount = expense.Amount,
                Currency = expense.Currency,
                Category = expense.Category,
                Date = expense.Date,
                Description = expense.Description
            });
        }

        [HttpPut("{id}")]
        public IActionResult UpdateExpense(int id, [FromBody] UpdateExpenseDto expenseDto)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var existingExpense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == userId);
            if (existingExpense == null) return NotFound();

            existingExpense.Amount = expenseDto.Amount;
            existingExpense.Currency = expenseDto.Currency;
            existingExpense.Category = expenseDto.Category;
            existingExpense.Date = expenseDto.Date;
            existingExpense.Description = expenseDto.Description;

            _context.SaveChanges();

            return Ok(new ExpenseDto
            {
                Id = existingExpense.Id,
                Amount = existingExpense.Amount,
                Currency = existingExpense.Currency,
                Category = existingExpense.Category,
                Date = existingExpense.Date,
                Description = existingExpense.Description
            });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteExpense(int id)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized();
            }

            var expense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == userId);
            if (expense == null) return NotFound(new { Message = "Expense not found" });

            _context.Expenses.Remove(expense);
            _context.SaveChanges();

            return Ok(new { Message = "Expense deleted successfully" });
        }
    }
}