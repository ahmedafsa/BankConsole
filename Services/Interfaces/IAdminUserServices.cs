using MODELS;


namespace BankSystem.Services.Interfaces
{
    public interface IAdminUserServices
    {
        User CurrentUser { get; }

        bool AddUser(string fullName, string username, string password, Userrole role=Userrole.Client);
        bool DeleteUser(int id);
        bool ChangeUsername(string username, string newUsername);
        bool ChangePassword(string username, string password, string newPassword);
    }
}
