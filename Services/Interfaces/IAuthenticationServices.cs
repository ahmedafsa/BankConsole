using MODELS;


namespace BankSystem.Services.Interfaces
{
    public interface IAuthenticationServices
    {
        User? CurrentUser { get; }

        bool Login(string username, string password);
        void Logout();
    }

}
