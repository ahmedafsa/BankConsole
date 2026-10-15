using BankSystem.DAL;
using BankSystem.Services.Interfaces;
using MODELS;

namespace BankSystem.Services.Classes
{
    public class AuthenticationServices : IAuthenticationServices
    {
        protected readonly BankDbContext _context;
        public AuthenticationServices(BankDbContext context)
        {
            _context = context;
        }
        public User? CurrentUser { get; protected set; } 
        public bool Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Username == username);
            if (user is null || !VarifyPassword(password, user.PasswordHash))
                return false;

            CurrentUser = user;
            return true;
        }
        public void Logout()
        {
            CurrentUser = null;
        }

        protected static bool VarifyPassword(string password, string storedHash)
        {
            return BCrypt.Net.BCrypt.Verify(password, storedHash);
        }
    }
}
