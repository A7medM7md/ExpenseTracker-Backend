namespace ExpenseTracker.Dtos
{
    public class CreateExpenseDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
