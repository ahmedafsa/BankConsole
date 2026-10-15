using MODELS;


namespace BankSystem.Services.Interfaces
{
    public interface IAdminUserServices : IUserAuthorizationServices
    {
        User CurrentUser { get; }

        bool AddUser(string fullName, string username, string password, Userrole role=Userrole.Client);
        bool DeleteUser(int id);
    }
}
