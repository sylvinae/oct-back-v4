using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using API.Entities.User;
using Newtonsoft.Json;

namespace API.Entities.Expense
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
        [Column(TypeName = "timestamp without time zone")]
        public DateTime ExpenseDate { get; set; }
        public ICollection<ExpenseItemEntity>? ExpenseItems { get; set; } = [];
    }
}
