using BankSystem.DAL;
using BankSystem.Services.Classes;
using MODELS;

namespace BankSystem.Menus
{
    internal sealed class SessionAdminUserServices : AdminUserServices
    {
        public SessionAdminUserServices(BankDbContext context, User currentUser)
            : base(context)
        {
            CurrentUser = currentUser;
        }

        public void SetCurrentUser(User currentUser) => CurrentUser = currentUser;
    }

    internal sealed class SessionAdminAccountServices : AdminAccountServices
    {
        public SessionAdminAccountServices(BankDbContext context, User currentUser)
            : base(context)
        {
            CurrentUser = currentUser;
        }

        public void SetCurrentUser(User currentUser) => CurrentUser = currentUser;
    }

    internal sealed class SessionUserAuthorizationServices : UserAuthorizationServices
    {
        public SessionUserAuthorizationServices(BankDbContext context, User currentUser)
            : base(context)
        {
            CurrentUser = currentUser;
        }

        public void SetCurrentUser(User currentUser) => CurrentUser = currentUser;
    }
}
