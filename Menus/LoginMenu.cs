using BankSystem.DAL;
using BankSystem.Services.Classes;
using BankSystem.Services.Interfaces;
using MODELS;
using Spectre.Console;

namespace BankSystem.Menus
{
    internal sealed class LoginMenu
    {
        private readonly BankDbContext _context;
        private readonly AuthenticationServices _auth;
        private readonly TransactionServices _transactions;

        public LoginMenu(BankDbContext context, AuthenticationServices auth)
        {
            _context = context;
            _auth = auth;
            _transactions = new TransactionServices(context);
        }

        public void Run()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Panel("[bold]Welcome to the Bank System[/]\n[grey]Please log in to continue.[/]")
                {
                    Header = new PanelHeader(" Bank System "),
                    Border = BoxBorder.Rounded
                });

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Main menu:")
                        .AddChoices("Login", "Exit"));

                if (choice == "Exit")
                    return;

                TryLogin();
            }
        }

        private void TryLogin()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold]Login[/]"));

            string username = MenuHelpers.PromptNonEmpty("Username:");
            string password = MenuHelpers.PromptPassword("Password:");

            bool ok;
            try
            {
                ok = _auth.Login(username, password);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
                MenuHelpers.Pause();
                return;
            }

            if (!ok || _auth.CurrentUser is null)
            {
                AnsiConsole.MarkupLine("[red]Invalid username or password.[/]");
                var retry = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("What next?")
                        .AddChoices("Retry login", "Back to main menu"));
                if (retry == "Retry login")
                    TryLogin();
                return;
            }

            User user = _auth.CurrentUser;
            RouteByRole(user);
        }

        private void RouteByRole(User user)
        {
            if (user.Role == Userrole.Admin)
            {
                var adminUsers = new SessionAdminUserServices(_context, user);
                var adminAccounts = new SessionAdminAccountServices(_context, user);
                var adminMenu = new AdminMenu(_context, _auth, adminUsers, adminAccounts, _transactions);
                adminMenu.Run();
            }
            else
            {
                var userServices = new SessionUserAuthorizationServices(_context, user);
                var clientMenu = new ClientMenu(_context, _auth, userServices, _transactions);
                clientMenu.Run();
            }

            if (_auth.CurrentUser is not null)
                _auth.Logout();
        }
    }
}
