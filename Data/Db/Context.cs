using Data.Entities.Expense;
using Data.Entities.Invoice;
using Data.Entities.Item;
using Data.Entities.User;
using Data.Item.ItemHistory;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Data.Db
{
    public class Context : IdentityDbContext<UserEntity, IdentityRole<Guid>, Guid>
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.UseSqlite("Data Source=../api/app.db");
        }

        // DbSets for your entities
        // public DbSet<UserEntity> Users { get; set; } = null!;
        public DbSet<InvoiceEntity> Invoices { get; set; } = null!;
        public DbSet<InvoiceItemEntity> InvoiceItems { get; set; } = null!;
        public DbSet<ItemEntity> Items { get; set; } = null!;
        public DbSet<ItemHistoryEntity> ItemHistories { get; set; } = null!;
        public DbSet<ExpenseEntity> Expenses { get; set; } = null!;
        public DbSet<ExpenseItemEntity> ExpenseItemEntities { get; set; } = null!;
    }
}
