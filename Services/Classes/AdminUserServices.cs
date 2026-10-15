using BankSystem.DAL;
using BankSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MODELS;
using System.ComponentModel.DataAnnotations;

namespace BankSystem.Services.Classes
{
    public class AdminUserServices : IAdminUserServices
    {
        protected readonly BankDbContext _context;

        public AdminUserServices(BankDbContext context)
        {
            _context = context;
        }

        public User CurrentUser { get; protected set; }

        private void EnsureAdmin(string message)
        {
            if (CurrentUser is null || CurrentUser.Role != Userrole.Admin)
                throw new UnauthorizedAccessException(message);
        }

        public bool AddUser(string fullName, string username, string password, Userrole role)
        {
            EnsureAdmin("Only an admin can add users.");

            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Username cannot be empty.");

            username = username.Trim();

            if (_context.Users.Any(u => u.Username == username))
                throw new ValidationException($"Username '{username}' already exists.");

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
                throw new ValidationException("Password must be at least 6 characters.");

            var user = new User
            {
                FullName = fullName,
                Username = username,
                PasswordHash = HashPassword(password),
                Role = role
            };

            Validator.ValidateObject(user, new ValidationContext(user), validateAllProperties: true);

            _context.Users.Add(user);
            int affectedRows = _context.SaveChanges();

            return affectedRows > 0;
        }

        public bool DeleteUser(int id)
        {
            EnsureAdmin("Only an admin can delete users.");

            if (id == CurrentUser.Id)
                throw new ValidationException("You cannot delete your own account.");

            if (!_context.Users.Any(u => u.Id == id))
                throw new ValidationException("Account is not found.");

            int affectedRows = _context.Users.Where(u => u.Id == id).ExecuteDelete();
            return affectedRows > 0;
        }

        public bool ChangeUsername(string username, string newUsername)
        {
            EnsureAdmin("Only an admin can change usernames.");

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(newUsername))
                throw new ValidationException("Username cannot be empty.");

            newUsername = newUsername.Trim();

            var target = _context.Users.FirstOrDefault(u => u.Username == username);
            if (target is null)
                throw new ValidationException("User is not found.");

            if (_context.Users.Any(u => u.Username == newUsername))
                throw new ValidationException("New username is already taken.");

            int affectedRows = _context.Users
                .Where(u => u.Id == target.Id)
                .ExecuteUpdate(u => u.SetProperty(x => x.Username, newUsername));

            if (affectedRows > 0 && target.Id == CurrentUser.Id)
                CurrentUser.Username = newUsername;

            return affectedRows > 0;
        }

        public bool ChangePassword(string username, string password, string newPassword)
        {
            EnsureAdmin("Only an admin can change passwords.");

            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("Username cannot be empty.");

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                throw new ValidationException("Password must be at least 6 characters.");

            var target = _context.Users.FirstOrDefault(u => u.Username == username);
            if (target is null)
                throw new ValidationException("User is not found.");

            if (target.Id == CurrentUser.Id &&
                !BCrypt.Net.BCrypt.Verify(password, CurrentUser.PasswordHash))
                throw new ValidationException("Current password is incorrect.");

            string hashedPassword = HashPassword(newPassword);

            int affectedRows = _context.Users
                .Where(u => u.Id == target.Id)
                .ExecuteUpdate(u => u.SetProperty(x => x.PasswordHash, hashedPassword));

            if (affectedRows > 0 && target.Id == CurrentUser.Id)
                CurrentUser.PasswordHash = hashedPassword;

            return affectedRows > 0;
        }
        public static string HashPassword(string password)
            => BCrypt.Net.BCrypt.HashPassword(password);
    }
}