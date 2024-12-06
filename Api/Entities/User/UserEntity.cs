using System.ComponentModel.DataAnnotations;
using API.Entities.Expense;
using API.Entities.Invoice;
using API.Item.ItemHistory;
using Microsoft.AspNetCore.Identity;

namespace API.Entities.User;

public class UserEntity : IdentityUser<Guid>
{
    [Required]
    public string FirstName { get; set; } = null!;

    public string? MiddleName { get; set; }

    [Required]
    public string LastName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public ICollection<InvoiceEntity>? Invoices { get; set; } = [];
    public ICollection<ExpenseEntity>? Expenses { get; set; } = [];
    public ICollection<ItemHistoryEntity>? ItemHistories { get; set; } = [];
}
