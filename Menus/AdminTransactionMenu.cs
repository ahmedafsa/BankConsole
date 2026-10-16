using BankSystem.Services.Classes;
using MODELS;
using Spectre.Console;

namespace BankSystem.Menus
{
    internal sealed class AdminTransactionMenu
    {
        private readonly TransactionServices _tx;

        public AdminTransactionMenu(TransactionServices transactions)
        {
            _tx = transactions;
        }

        public void Run()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold]Transaction Management (Admin)[/]"));
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose an action:")
                        .AddChoices(
                            "Deposit",
                            "Withdraw",
                            "Transfer between accounts",
                            "View all transactions",
                            "View transactions by account",
                            "View deposits",
                            "View withdrawals",
                            "View transfers",
                            "Look up a transaction by ID",
                            "Back"));
                try
                {
                    switch (choice)
                    {
                        case "Deposit": Deposit(); break;
                        case "Withdraw": Withdraw(); break;
                        case "Transfer between accounts": Transfer(); break;
                        case "View all transactions": ViewAll(); break;
                        case "View transactions by account": ByAccount(); break;
                        case "View deposits": Filtered("Deposits"); break;
                        case "View withdrawals": Filtered("Withdrawals"); break;
                        case "View transfers": Filtered("Transfers"); break;
                        case "Look up a transaction by ID": LookupById(); break;
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

        private void Deposit()
        {
            int accountId = MenuHelpers.AskInt("Account Id:");
            decimal amount = MenuHelpers.AskAmount("Amount (> 0):");
            if (!MenuHelpers.ConfirmAction($"Deposit {amount:C} into account #{accountId}?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                var t = _tx.Deposit(accountId, amount);
                MenuHelpers.ShowReceipt(t);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void Withdraw()
        {
            int accountId = MenuHelpers.AskInt("Account Id:");
            decimal amount = MenuHelpers.AskAmount("Amount (> 0):");
            if (!MenuHelpers.ConfirmAction($"Withdraw {amount:C} from account #{accountId}?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                var t = _tx.Withdraw(accountId, amount);
                MenuHelpers.ShowReceipt(t);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void Transfer()
        {
            int from = MenuHelpers.AskInt("Source Account Id:");
            int to = MenuHelpers.AskInt("Destination Account Id:");
            if (from == to)
            {
                AnsiConsole.MarkupLine("[red]Cannot transfer to the same account.[/]");
                MenuHelpers.Pause();
                return;
            }
            decimal amount = MenuHelpers.AskAmount("Amount (> 0):");
            if (amount >= 10000 && !MenuHelpers.ConfirmAction($"Large transfer of {amount:C} — type-confirm again to proceed."))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            if (!MenuHelpers.ConfirmAction($"Transfer {amount:C} from #{from} to #{to}?"))
            {
                AnsiConsole.MarkupLine("[grey]Cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                var t = _tx.Transfer(from, to, amount);
                MenuHelpers.ShowReceipt(t);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ViewAll()
        {
            try
            {
                MenuHelpers.ShowTransactionsTable(_tx.ShowAllTransactions(), "All Transactions");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ByAccount()
        {
            int accountId = MenuHelpers.AskInt("Account Id:");
            try
            {
                MenuHelpers.ShowTransactionsTable(
                    _tx.ShowTransactionsByAccountId(accountId), $"Statement — Account #{accountId}");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void Filtered(string kind)
        {
            AnsiConsole.MarkupLine("[grey]Leave blank for all accounts (admin report).[/]");
            int? filter = MenuHelpers.AskOptionalInt("Account Id (optional):");
            try
            {
                List<Transaction> list = kind switch
                {
                    "Deposits" => _tx.ShowDeposits(filter),
                    "Withdrawals" => _tx.ShowWithdraws(filter),
                    _ => _tx.ShowTransfers(filter),
                };
                MenuHelpers.ShowTransactionsTable(list,
                    filter.HasValue ? $"{kind} — Account #{filter}" : $"All {kind.ToLower()}");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void LookupById()
        {
            int id = MenuHelpers.AskInt("Transaction Id:");
            try
            {
                var t = _tx.ShowTransactionById(id);
                if (t is null)
                    AnsiConsole.MarkupLine("[yellow]Transaction not found.[/]");
                else
                    MenuHelpers.ShowTransactionDetail(t);
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }
    }
}
