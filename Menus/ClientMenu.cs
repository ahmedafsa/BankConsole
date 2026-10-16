using BankSystem.DAL;
using BankSystem.Services.Classes;
using Microsoft.EntityFrameworkCore;
using MODELS;
using Spectre.Console;

namespace BankSystem.Menus
{
    internal sealed class ClientMenu
    {
        private readonly BankDbContext _context;
        private readonly AuthenticationServices _auth;
        private readonly SessionUserAuthorizationServices _me;
        private readonly TransactionServices _tx;

        public ClientMenu(
            BankDbContext context,
            AuthenticationServices auth,
            SessionUserAuthorizationServices userServices,
            TransactionServices transactions)
        {
            _context = context;
            _auth = auth;
            _me = userServices;
            _tx = transactions;
        }

        public void Run()
        {
            if (_auth.CurrentUser is null)
            {
                AnsiConsole.MarkupLine("[red]Access denied. Please log in.[/]");
                MenuHelpers.Pause();
                return;
            }

            while (true)
            {
                AnsiConsole.Clear();
                MenuHelpers.ShowHeader(_auth.CurrentUser.Username, _auth.CurrentUser.Role.ToString());
                AnsiConsole.Write(new Rule("[bold]Client Dashboard[/]"));

                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose a section:")
                        .AddChoices(
                            "My Accounts",
                            "Transactions",
                            "Profile",
                            "Logout"));

                switch (choice)
                {
                    case "My Accounts": ShowMyAccounts(); break;
                    case "Transactions": RunTransactions(); break;
                    case "Profile": RunProfile(); break;
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

        // ---- A. My Accounts ----

        private List<Account> LoadMyAccounts()
        {
            try
            {
                return _me.GetMyAccounts();
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
                return new List<Account>();
            }
        }

        private void ShowMyAccounts()
        {
            AnsiConsole.Clear();
            AnsiConsole.Write(new Rule("[bold]My Accounts[/]"));
            var accounts = LoadMyAccounts();
            if (accounts.Count == 0)
                AnsiConsole.MarkupLine("[yellow]You have no accounts. Please contact an administrator.[/]");
            else
                MenuHelpers.ShowAccountsTable(accounts);
            MenuHelpers.Pause();
        }

        private HashSet<int> OwnedIds() => LoadMyAccounts().Select(a => a.Id).ToHashSet();

        private int? PickOwnedAccount(string promptTitle = "Select account:")
        {
            var accounts = LoadMyAccounts();
            if (accounts.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]You have no accounts. Please contact an administrator.[/]");
                MenuHelpers.Pause();
                return null;
            }
            MenuHelpers.ShowAccountsTable(accounts);
            int id = MenuHelpers.AskInt(promptTitle + " Enter Account Id:");
            if (!accounts.Any(a => a.Id == id))
            {
                AnsiConsole.MarkupLine("[red]Access denied: that account does not belong to you.[/]");
                MenuHelpers.Pause();
                return null;
            }
            return id;
        }

        // ---- B. Transactions ----

        private void RunTransactions()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold]My Transactions[/]"));
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose an action:")
                        .AddChoices(
                            "Deposit into my account",
                            "Withdraw from my account",
                            "Transfer to another account",
                            "View my account statement",
                            "View my deposits",
                            "View my withdrawals",
                            "View my transfers",
                            "Look up one of my transactions",
                            "Back"));
                try
                {
                    switch (choice)
                    {
                        case "Deposit into my account": ClientDeposit(); break;
                        case "Withdraw from my account": ClientWithdraw(); break;
                        case "Transfer to another account": ClientTransfer(); break;
                        case "View my account statement": ClientStatement(); break;
                        case "View my deposits": ClientDeposits(); break;
                        case "View my withdrawals": ClientWithdrawals(); break;
                        case "View my transfers": ClientTransfers(); break;
                        case "Look up one of my transactions": ClientLookup(); break;
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

        private void ClientDeposit()
        {
            var id = PickOwnedAccount();
            if (id is null) return;
            decimal amount = MenuHelpers.AskAmount("Amount (> 0):");
            if (!MenuHelpers.ConfirmAction($"Deposit {amount:C} into account #{id}?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                var t = _tx.Deposit(id.Value, amount);
                MenuHelpers.ShowReceipt(t, LoadMyAccounts().FirstOrDefault(a => a.Id == id.Value)?.Balance);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ClientWithdraw()
        {
            var id = PickOwnedAccount();
            if (id is null) return;
            decimal amount = MenuHelpers.AskAmount("Amount (> 0):");
            if (!MenuHelpers.ConfirmAction($"Withdraw {amount:C} from account #{id}?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                var t = _tx.Withdraw(id.Value, amount);
                MenuHelpers.ShowReceipt(t, LoadMyAccounts().FirstOrDefault(a => a.Id == id.Value)?.Balance);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ClientTransfer()
        {
            var from = PickOwnedAccount("Select source account:");
            if (from is null) return;
            int to = MenuHelpers.AskInt("Destination Account Id (can belong to another user):");
            if (to == from.Value)
            {
                AnsiConsole.MarkupLine("[red]Cannot transfer to the same account.[/]");
                MenuHelpers.Pause();
                return;
            }
            
            if (!_context.Accounts.AsNoTracking().Any(a => a.Id == to))
            {
                AnsiConsole.MarkupLine($"[red]Destination account #{to} not found. Transfer cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            decimal amount = MenuHelpers.AskAmount("Amount (> 0):");
            if (!MenuHelpers.ConfirmAction($"Transfer {amount:C} from #{from} to #{to}?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                var t = _tx.Transfer(from.Value, to, amount);
                MenuHelpers.ShowReceipt(t, LoadMyAccounts().FirstOrDefault(a => a.Id == from.Value)?.Balance);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ClientStatement()
        {
            var id = PickOwnedAccount();
            if (id is null) return;
            try
            {
               
                MenuHelpers.ShowTransactionsTable(
                    _tx.ShowTransactionsByAccountId(id.Value), $"Statement — Account #{id}");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ClientDeposits()
        {
            var id = PickOwnedAccount();
            if (id is null) return;
            try
            {
               
                MenuHelpers.ShowTransactionsTable(_tx.ShowDeposits(id.Value), $"My Deposits — Account #{id}");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ClientWithdrawals()
        {
            var id = PickOwnedAccount();
            if (id is null) return;
            try
            {
                MenuHelpers.ShowTransactionsTable(_tx.ShowWithdraws(id.Value), $"My Withdrawals — Account #{id}");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ClientTransfers()
        {
            var id = PickOwnedAccount();
            if (id is null) return;
            try
            {
                MenuHelpers.ShowTransactionsTable(_tx.ShowTransfers(id.Value), $"My Transfers — Account #{id}");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ClientLookup()
        {
            int id = MenuHelpers.AskInt("Transaction Id:");
            try
            {
                var t = _tx.ShowTransactionById(id);
                if (t is null)
                {
                    AnsiConsole.MarkupLine("[yellow]Transaction not found.[/]");
                }
                else
                {
                    var owned = OwnedIds();
                    bool mine = owned.Contains(t.AccountId)
                        || (t.TargetAccountId.HasValue && owned.Contains(t.TargetAccountId.Value));
                    if (!mine)
                        AnsiConsole.MarkupLine("[red]Access denied: that transaction does not involve your accounts.[/]");
                    else
                        MenuHelpers.ShowTransactionDetail(t);
                }
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        // ---- C. Profile ----

        private void RunProfile()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold]My Profile[/]"));
                AnsiConsole.MarkupLine($"Username: [green]{Markup.Escape(_me.CurrentUser.Username)}[/]");
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose an action:")
                        .AddChoices("Change my username", "Change my password", "Back"));
                try
                {
                    switch (choice)
                    {
                        case "Change my username": ChangeMyUsername(); break;
                        case "Change my password": ChangeMyPassword(); break;
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

        private void ChangeMyUsername()
        {
            AnsiConsole.MarkupLine($"Current username: [green]{Markup.Escape(_me.CurrentUser.Username)}[/]");
            string next = MenuHelpers.PromptNonEmpty("New username (min 3 chars):");
            
            string current = _me.CurrentUser.Username;
            if (!MenuHelpers.ConfirmAction($"Change username to '{next}'?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                if (_me.ChangeUsername(current, next))
                {
                  
                    if (_auth.CurrentUser is not null)
                        _auth.CurrentUser.Username = _me.CurrentUser.Username;
                    AnsiConsole.MarkupLine("[green]Username changed. Use the new name next login.[/]");
                }
                else
                    AnsiConsole.MarkupLine("[red]Username could not be changed.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ChangeMyPassword()
        {
            string currentPw = MenuHelpers.PromptPassword("Current password:");
            string next = MenuHelpers.PromptPasswordWithConfirm();
            string current = _me.CurrentUser.Username;
            try
            {
                if (_me.ChangePassword(current, currentPw, next))
                    AnsiConsole.MarkupLine("[green]Password changed.[/]");
                else
                    AnsiConsole.MarkupLine("[red]Password could not be changed.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }
    }
}
