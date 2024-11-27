using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Data.Entities.User;
using Newtonsoft.Json;

namespace Data.Entities.Expense
{
    public class ExpenseEntity
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public UserEntity User { get; set; } = null!;

        [Required]
        public decimal TotalCost { get; set; }

        [Required]
        public string ExpenseDate { get; set; } = null!;
        public ICollection<ExpenseItemEntity>? ExpenseItems { get; set; } = [];
    }
}
