namespace API.Models.Expense;

public class ExpenseModel
{
    public Guid? UserId { get; set; }
    public DateTime ExpenseDate { get; set; }
    public decimal TotalCost { get; set; }
    public ICollection<ExpenseItemModel> ExpenseItems { get; set; } = [];
}

public class CreateExpenseModel : ExpenseModel;

public class ResponseExpenseModel : ExpenseModel
{
    public Guid? Id { get; set; }
}