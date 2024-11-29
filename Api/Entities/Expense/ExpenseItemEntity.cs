using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace API.Entities.Expense
{
    public class ExpenseItemEntity
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        public Guid ExpenseId { get; set; }

        [ForeignKey("ExpenseId")]
        public ExpenseEntity Expense { get; set; } = null!;
        public required string Details { get; set; }
        public decimal Amount { get; set; }
    }
}
