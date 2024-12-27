namespace API.Models.Expense;

public class ExpenseItemModel
{
    public Guid? ExpenseId { get; set; }
    public string Details { get; set; } = null!;
    public decimal Amount { get; set; }
}