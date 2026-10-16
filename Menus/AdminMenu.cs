using BankSystem.DAL;
using BankSystem.Services.Classes;
using Spectre.Console;

namespace BankSystem.Menus
{
    internal sealed class AdminMenu
    {
        private readonly BankDbContext _context;
        private readonly AuthenticationServices _auth;
        private readonly SessionAdminUserServices _adminUsers;
        private readonly SessionAdminAccountServices _adminAccounts;
        private readonly TransactionServices _transactions;

        public AdminMenu(
            BankDbContext context,
            AuthenticationServices auth,
            SessionAdminUserServices adminUsers,
            SessionAdminAccountServices adminAccounts,
            TransactionServices transactions)
        {
            _context = context;
            _auth = auth;
            _adminUsers = adminUsers;
            _adminAccounts = adminAccounts;
            _transactions = transactions;
        }

        public void Run()
        {
            if (_auth.CurrentUser is null)
            {
                AnsiConsole.MarkupLine("[red]Access denied. Please log in.[/]");
                MenuHelpers.Pause();
                return;
            }

            var userMenu = new AdminUserMenu(_context, _adminUsers);
            var accountMenu = new AdminAccountMenu(_context, _adminAccounts);
            var txMenu = new AdminTransactionMenu(_transactions);

            while (true)
            {
                AnsiConsole.Clear();
                MenuHelpers.ShowHeader(_auth.CurrentUser.Username, _auth.CurrentUser.Role.ToString());
                AnsiConsole.Write(new Rule("[bold]Administrator Dashboard[/]"));

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose a section:")
                        .AddChoices(
                            "User Management",
                            "Account Management",
                            "Transaction Management",
                            "Logout"));

                switch (choice)
                {
                    case "User Management": userMenu.Run(); break;
                    case "Account Management": accountMenu.Run(); break;
                    case "Transaction Management": txMenu.Run(); break;
                    default:
                        if (MenuHelpers.ConfirmAction("Log out?"))
                        {
                            _auth.Logout();
                            AnsiConsole.MarkupLine("[green]Logged out successfully.[/]");
                            MenuHelpers.Pause();
                            return;
                        }
                        break;
                }
            }
        }
    }
}
