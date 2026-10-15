using BankSystem.Services.Interfaces;
using MODELS;
using System.ComponentModel.DataAnnotations;
using BankSystem.DAL;
using Microsoft.EntityFrameworkCore;

namespace BankSystem.Services.Classes
{
    public class UserAuthorizationServices : IUserAuthorizationServices
    {
        protected readonly BankDbContext _context;

        public UserAuthorizationServices(BankDbContext context)
        {
            _context = context;
        }

        public User CurrentUser { get; protected set; }

        public bool ChangeUsername(string username, string newUsername)
        {
            if (CurrentUser == null)
                throw new ValidationException("Something went wrong. There is no current user.");

            if (username != CurrentUser.Username)
                throw new ValidationException("You can only change your own username.");

            if (string.IsNullOrWhiteSpace(newUsername))
                throw new ValidationException("New username cannot be empty.");

            newUsername = newUsername.Trim();

            if (_context.Users.Any(u => u.Username == newUsername))
                throw new ValidationException("New username is already taken.");

            int affectedRows = _context.Users
                .Where(u => u.Username == username)
                .ExecuteUpdate(u => u.SetProperty(x => x.Username, newUsername));

            if (affectedRows > 0)
                CurrentUser.Username = newUsername;

            return affectedRows > 0;
        }

        public bool ChangePassword(string username, string password, string newPassword)
        {
            if (CurrentUser == null)
                throw new ValidationException("Something went wrong. There is no current user.");

            if (username != CurrentUser.Username)
                throw new ValidationException("You can only change your own password.");

            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(newPassword))
                throw new ValidationException("Password cannot be empty.");

            if (!BCrypt.Net.BCrypt.Verify(password, CurrentUser.PasswordHash))
                throw new ValidationException("Current password is incorrect.");

            string hashedPassword = HashPassword(newPassword);

            int affectedRows = _context.Users
                .Where(u => u.Username == username)
                .ExecuteUpdate(u => u.SetProperty(x => x.PasswordHash, hashedPassword));

            if (affectedRows > 0)
                CurrentUser.PasswordHash = hashedPassword;

            return affectedRows > 0;
        }

        public List<Account> GetMyAccounts()
        {
            if (CurrentUser == null)
                throw new ValidationException("Something went wrong. There is no current user.");

            return _context.Accounts
                .AsNoTracking()
                .Where(a => a.UserId == CurrentUser.Id)
                .ToList();
        }

        public static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);
    }
}