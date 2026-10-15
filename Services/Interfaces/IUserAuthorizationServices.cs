using MODELS;


namespace BankSystem.Services.Interfaces
{
    public interface IUserAuthorizationServices
    {
        User CurrentUser { get; }

        bool ChangeUsername(string username, string newUsername);
        bool ChangePassword(string username, string password, string newPassword);
        List<Account> GetMyAccounts();

    }
}
