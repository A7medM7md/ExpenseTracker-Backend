﻿namespace ExpenseTracker.Dtos
{
    public class UpdateExpenseDto
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
    }
}
