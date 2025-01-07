using System.ComponentModel.DataAnnotations;
using API.Entities.Expense;
using API.Entities.Invoice;
using API.Entities.Products;
using Microsoft.AspNetCore.Identity;

namespace API.Entities.User;

public class UserEntity : IdentityUser<Guid>
{
    [MaxLength(50)] [Required] public string FirstName { get; set; } = null!;

    [MaxLength(50)] public string? MiddleName { get; set; }

    [MaxLength(50)] [Required] public string LastName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public ICollection<InvoiceEntity> Invoices { get; set; } = [];

    public ICollection<ExpenseEntity> Expenses { get; set; } = [];
    public ICollection<ProductHistoryEntity> ProductHistories { get; set; } = [];
}