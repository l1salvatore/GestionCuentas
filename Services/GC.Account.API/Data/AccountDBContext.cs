using Microsoft.EntityFrameworkCore;
using GC.Account.API.Models;
using Microsoft.EntityFrameworkCore.Design;

namespace GC.Account.API.Data
{
     public class AccountDbContextFactory 
        : IDesignTimeDbContextFactory<AccountDbContext>
    {
        public AccountDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AccountDbContext>();

            optionsBuilder.UseSqlServer(
                "Server=.;Database=AccountDB;Trusted_Connection=True;TrustServerCertificate=True");

            return new AccountDbContext(optionsBuilder.Options);
        }
    }
    
    public class AccountDbContext : DbContext
    {
        public AccountDbContext(DbContextOptions<AccountDbContext> options) : base(options) { }

        public DbSet<Models.Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuración de precisión para campos decimales
            modelBuilder.Entity<Models.Account>()
                .Property(a => a.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Amount)
                .HasPrecision(18, 2);

            // Relación Uno a Muchos: Una Cuenta tiene muchas Transacciones
            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Account)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.AccountId);

            modelBuilder.Entity<Models.Account>()
                .HasIndex(a => a.UserId)
                .IsUnique();
        }
    }
}