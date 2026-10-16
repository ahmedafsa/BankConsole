using BankSystem.DAL;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace BankSystem.Menus
{
    internal sealed class AdminAccountMenu
    {
        private readonly BankDbContext _context;
        private readonly SessionAdminAccountServices _accounts;

        public AdminAccountMenu(BankDbContext context, SessionAdminAccountServices accounts)
        {
            _context = context;
            _accounts = accounts;
        }

        public void Run()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold]Account Management[/]"));
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose an action:")
                        .AddChoices(
                            "Create account",
                            "Delete account",
                            "List all accounts",
                            "Find accounts by username",
                            "Back"));
                try
                {
                    switch (choice)
                    {
                        case "Create account": CreateAccount(); break;
                        case "Delete account": DeleteAccount(); break;
                        case "List all accounts": ListAll(); break;
                        case "Find accounts by username": FindByUsername(); break;
                        default: return;
                    }
                }
                catch (Exception ex)
                {
                    MenuHelpers.HandleException(ex);
                    MenuHelpers.Pause();
                }
            }
        }

        private void CreateAccount()
        {
            string username = MenuHelpers.PromptNonEmpty("Owner username:");
            decimal balance = MenuHelpers.AskBalance("Initial balance (>= 0):");
            if (!MenuHelpers.ConfirmAction($"Create account with balance {balance:C} for '{username}'?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                if (_accounts.AddAccount(balance, username))
                    AnsiConsole.MarkupLine("[green]Account created.[/]");
                else
                    AnsiConsole.MarkupLine("[red]Account could not be created.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void DeleteAccount()
        {
            ListAllQuick();
            int id = MenuHelpers.AskInt("Enter Account Id to delete:");
            if (!MenuHelpers.ConfirmAction($"Delete account #{id}?"))
            {
                AnsiConsole.MarkupLine("[grey]Deletion cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                if (_accounts.DeleteAccount(id))
                    AnsiConsole.MarkupLine("[green]Account deleted.[/]");
                else
                    AnsiConsole.MarkupLine("[red]Account could not be deleted.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ListAll()
        {
            try
            {
                var accounts = _accounts.GetAllAccounts();
                MenuHelpers.ShowAccountsTable(accounts, ResolveUsernames());
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void FindByUsername()
        {
            string username = MenuHelpers.PromptNonEmpty("Username:");
            try
            {
                var accounts = _accounts.GetAccountsByUsername(username);
                if (accounts.Count == 0)
                    AnsiConsole.MarkupLine($"[yellow]User '{Markup.Escape(username)}' has no accounts.[/]");
                else
                    MenuHelpers.ShowAccountsTable(accounts);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ListAllQuick()
        {
            try
            {
                MenuHelpers.ShowAccountsTable(_accounts.GetAllAccounts(), ResolveUsernames());
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
        }

        private Dictionary<int, string> ResolveUsernames()
        {
            return _context.Users.AsNoTracking().ToDictionary(u => u.Id, u => u.Username);
        }
    }
}
