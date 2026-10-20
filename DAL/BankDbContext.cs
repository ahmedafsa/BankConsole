using Microsoft.EntityFrameworkCore;
using MODELS;

namespace BankSystem.DAL
{
    public class BankDbContext : DbContext
    {
        
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("Server=localhost;Database=BankDB;Integrated Security=True;TrustServerCertificate=True;");
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public void EnsureAdminSeeded()
        {
            if (Users.Any(u => u.Role == Userrole.Admin))
                return;

            if (Users.Any(u => u.Username == "admin"))
                return;

            Users.Add(new User
            {
                FullName = "Admin",
                Username = "admin",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin@123"),
                Role = Userrole.Admin
            });
            SaveChanges();
        }
        
    }

}
