using Microsoft.AspNetCore.Mvc;
using ExpenseTracker.Models.Entities;
using ExpenseTracker.Data;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var expenses = _context.Expenses.Where(e => e.UserId == userId).ToList();
            return Ok(expenses);
        }

        [HttpPost]
        public IActionResult AddExpense(Expense expense)
        {
            expense.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            _context.Expenses.Add(expense);
            _context.SaveChanges();
            return Ok(expense);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateExpense(int id, Expense expense)
        {
            var existingExpense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            if (existingExpense == null) return NotFound();

            existingExpense.Amount = expense.Amount;
            existingExpense.Currency = expense.Currency;
            existingExpense.Category = expense.Category;
            existingExpense.Date = expense.Date;
            existingExpense.Description = expense.Description;
            _context.SaveChanges();
            return Ok(existingExpense);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteExpense(int id)
        {
            var expense = _context.Expenses.FirstOrDefault(e => e.Id == id && e.UserId == int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value));
            if (expense == null) return NotFound();

            _context.Expenses.Remove(expense);
            _context.SaveChanges();
            return Ok();
        }
    }
}