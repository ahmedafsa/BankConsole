using BankSystem.DAL;
using BankSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MODELS;
using System.ComponentModel.DataAnnotations;

namespace BankSystem.Services.Classes
{
    public class AdminAccountServices : IAdminAccountServices
    {
        protected readonly BankDbContext _context;

        public AdminAccountServices(BankDbContext context)
        {
            _context = context;
        }

        public User CurrentUser { get; protected set; }

        private void EnsureAdmin(string message)
        {
            if (CurrentUser is null || CurrentUser.Role != Userrole.Admin)
                throw new UnauthorizedAccessException(message);
        }

        public bool AddAccount(decimal balance, string username)
        {
            EnsureAdmin("Only an admin can add accounts.");

            if (balance < 0)
                throw new ValidationException("Balance cannot be negative.");

            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Username cannot be empty.");

            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user is null)
                throw new ValidationException($"User '{username}' does not exist.");

            var account = new Account
            {
                Balance = balance,
                UserId = user.Id
            };

            Validator.ValidateObject(account, new ValidationContext(account), validateAllProperties: true);

            _context.Accounts.Add(account);
            int affectedRows = _context.SaveChanges();

            return affectedRows > 0;
        }

        public bool DeleteAccount(int id)
        {
            EnsureAdmin("Only an admin can delete accounts.");

            if (!_context.Accounts.Any(a => a.Id == id))
                throw new ValidationException("Account is not found.");

            int affectedRows = _context.Accounts.Where(a => a.Id == id).ExecuteDelete();

            return affectedRows > 0;
        }

        public List<Account> GetAllAccounts()
        {
            EnsureAdmin("Only an admin can view all accounts.");

            return _context.Accounts
                .AsNoTracking()
                .ToList();
        }

        public List<Account> GetAccountsByUsername(string username)
        {
            EnsureAdmin("Only an admin can manage accounts.");

            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Username cannot be empty.");

            username = username.Trim();

            if (!_context.Users.Any(u => u.Username == username))
                throw new ValidationException($"User '{username}' does not exist.");

            return _context.Accounts
                .AsNoTracking()
                .Where(a => a.User.Username == username)
                .ToList();
        }
    }
}