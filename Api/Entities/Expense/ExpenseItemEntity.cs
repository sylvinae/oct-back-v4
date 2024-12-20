using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities.Expense;

public class ExpenseItemEntity
{
    [Required] public Guid Id { get; init; } = Guid.NewGuid();

    [Required] public Guid ExpenseId { get; set; }

    [ForeignKey("ExpenseId")] public ExpenseEntity Expense { get; set; } = null!;

    [MaxLength(500)] [Required] public string Details { get; set; } = null!;

    public decimal Amount { get; set; }
}