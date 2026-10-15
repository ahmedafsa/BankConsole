using MODELS;

namespace BankSystem.Services.Interfaces
{
    public interface IAdminAccountServices
    {
        User CurrentUser { get; }

        bool AddAccount(decimal balance, string username);
        bool DeleteAccount(int id);
        List<Account> GetAllAccounts();
        List<Account> GetAccountsByUsername(string username);
    }
}
