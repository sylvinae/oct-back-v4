using API.Entities.Bundles;
using API.Entities.Expense;
using API.Entities.Invoice;
using API.Entities.Item;
using API.Entities.Products;
using API.Entities.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Db;

public class Context(DbContextOptions<Context> options)
    : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<ProductEntity> Products { get; set; } = null!;
    public DbSet<BundleEntity> Bundles { get; set; } = null!;
    public DbSet<BundleHistoryEntity> BundleHistories { get; set; } = null!;
    public DbSet<BundleItemEntity> BundleItems { get; set; } = null!;
    public DbSet<BundleItemHistoryEntity> BundleItemHistories { get; set; } = null!;
    public DbSet<ItemEntity> Items { get; set; } = null!;
    public DbSet<ItemHistoryEntity> ItemHistories { get; set; } = null!;
    public DbSet<InvoiceEntity> Invoices { get; set; } = null!;
    public DbSet<InvoiceItemEntity> InvoiceItems { get; set; } = null!;
    public DbSet<ExpenseEntity> Expenses { get; set; } = null!;
    public DbSet<ExpenseItemEntity> ExpenseItems { get; set; } = null!;
}