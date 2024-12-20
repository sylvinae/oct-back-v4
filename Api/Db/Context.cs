using API.Entities.Expense;
using API.Entities.Invoice;
using API.Entities.Item;
using API.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Db;

public class Context(DbContextOptions<Context> options)
    : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options)
{
    // DbSets for your entities
    public DbSet<InvoiceEntity> Invoices { get; set; } = null!;
    public DbSet<InvoiceItemEntity> InvoiceItems { get; set; } = null!;
    public DbSet<ItemEntity> Items { get; set; } = null!;
    public DbSet<ItemHistoryEntity> ItemHistories { get; set; } = null!;
    public DbSet<ExpenseEntity> Expenses { get; set; } = null!;
    public DbSet<ExpenseItemEntity> ExpenseItems { get; set; } = null!;
}