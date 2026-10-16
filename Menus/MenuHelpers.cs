using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore;
using MODELS;
using Spectre.Console;

namespace BankSystem.Menus
{
    internal static class MenuHelpers
    {
        public static string PromptNonEmpty(string message)
        {
            while (true)
            {
                var input = AnsiConsole.Prompt(
                    new TextPrompt<string>(message)
                        .AllowEmpty());
                if (!string.IsNullOrWhiteSpace(input))
                    return input.Trim();

                AnsiConsole.MarkupLine("[red]Value cannot be empty. Please try again.[/]");
            }
        }

        public static string PromptPassword(string message)
        {
            while (true)
            {
                var input = AnsiConsole.Prompt(
                    new TextPrompt<string>(message)
                        .Secret()
                        .AllowEmpty());
                if (!string.IsNullOrWhiteSpace(input))
                    return input;

                AnsiConsole.MarkupLine("[red]Password cannot be empty. Please try again.[/]");
            }
        }

        public static string PromptPasswordWithConfirm(string newPasswordMessage = "New password:")
        {
            while (true)
            {
                var first = PromptPassword(newPasswordMessage);
                var second = AnsiConsole.Prompt(
                    new TextPrompt<string>("Confirm new password:")
                        .Secret()
                        .AllowEmpty());
                if (first != second)
                {
                    AnsiConsole.MarkupLine("[red]Passwords do not match. Please try again.[/]");
                    continue;
                }
                if (first.Length < 6)
                {
                    AnsiConsole.MarkupLine("[red]Password must be at least 6 characters.[/]");
                    continue;
                }
                return first;
            }
        }

        public static int AskInt(string message)
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>(message);
                if (int.TryParse(input?.Trim(), out int value))
                    return value;

                AnsiConsole.MarkupLine("[red]Please enter a valid whole number.[/]");
            }
        }

        public static int? AskOptionalInt(string message)
        {
            var input = AnsiConsole.Ask<string>(message);
            if (string.IsNullOrWhiteSpace(input))
                return null;
            if (int.TryParse(input.Trim(), out int value))
                return value;

            AnsiConsole.MarkupLine("[red]Invalid number — showing results for all accounts.[/]");
            return null;
        }

        /// <summary>Prompts for a money amount strictly greater than zero.</summary>
        public static decimal AskAmount(string message = "Amount:")
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>(message);
                if (decimal.TryParse(input?.Trim(), out decimal value) && value > 0)
                    return value;

                AnsiConsole.MarkupLine("[red]Amount must be a number greater than zero.[/]");
            }
        }

        /// <summary>Prompts for an initial balance (zero or more).</summary>
        public static decimal AskBalance(string message = "Initial balance:")
        {
            while (true)
            {
                var input = AnsiConsole.Ask<string>(message);
                if (decimal.TryParse(input?.Trim(), out decimal value) && value >= 0)
                    return value;

                AnsiConsole.MarkupLine("[red]Balance must be a number greater than or equal to zero.[/]");
            }
        }

        public static bool ConfirmAction(string message)
            => AnsiConsole.Confirm(message);

        public static void Pause()
        {
            AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
            Console.ReadKey(intercept: true);
        }

        public static void ShowHeader(string username, string role)
        {
            AnsiConsole.Write(new Panel(
                $"[bold]Bank System[/]  |  Logged in as [green]{Markup.Escape(username)}[/] [[{Markup.Escape(role)}]]")
            {
                Header = new PanelHeader(" Dashboard "),
                Border = BoxBorder.Rounded
            });
        }

        public static void ShowAccountsTable(List<Account> accounts, IReadOnlyDictionary<int, string>? usernamesByUserId = null)
        {
            if (accounts.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No accounts found.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Account Id");
            table.AddColumn("Balance");
            table.AddColumn(usernamesByUserId is null ? "User Id" : "Owner");

            foreach (var a in accounts)
            {
                string owner = usernamesByUserId is not null && usernamesByUserId.TryGetValue(a.UserId, out var name)
                    ? Markup.Escape(name)
                    : a.UserId.ToString();
                table.AddRow(a.Id.ToString(), a.Balance.ToString("C"), owner);
            }

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[grey]Total: {accounts.Count} account(s).[/]");
        }

        public static void ShowUsersTable(List<User> users)
        {
            if (users.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No users found.[/]");
                return;
            }

            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Id");
            table.AddColumn("Username");
            table.AddColumn("Full Name");
            table.AddColumn("Role");

            foreach (var u in users)
                table.AddRow(u.Id.ToString(), Markup.Escape(u.Username), Markup.Escape(u.FullName), u.Role.ToString());

            AnsiConsole.Write(table);
            AnsiConsole.MarkupLine($"[grey]Total: {users.Count} user(s).[/]");
        }

        public static void ShowTransactionsTable(List<Transaction> transactions, string title = "Transactions", int pageSize = 15)
        {
            if (transactions.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No transactions found.[/]");
                return;
            }

            int totalPages = (transactions.Count + pageSize - 1) / pageSize;
            int page = 0;

            while (true)
            {
                AnsiConsole.Write(new Rule($"[bold]{Markup.Escape(title)}[/] — page {page + 1}/{totalPages}"));
                var table = new Table().Border(TableBorder.Rounded);
                table.AddColumn("Txn Id");
                table.AddColumn("Type");
                table.AddColumn("Amount");
                table.AddColumn("From Acct");
                table.AddColumn("To Acct");
                table.AddColumn("Date (UTC)");

                foreach (var t in transactions.Skip(page * pageSize).Take(pageSize))
                {
                    table.AddRow(
                        t.Id.ToString(),
                        t.Transactiontype.ToString(),
                        t.Amount.ToString("C"),
                        t.AccountId.ToString(),
                        t.TargetAccountId?.ToString() ?? "-",
                        t.CreatedAt.ToString("u"));
                }

                AnsiConsole.Write(table);
                AnsiConsole.MarkupLine($"[grey]Showing {Math.Min(page * pageSize + pageSize, transactions.Count)} of {transactions.Count}.[/]");

                if (totalPages <= 1)
                    return;

                var nav = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Navigate:")
                        .AddChoices(new[] { "Next page", "Previous page", "Back" }));
                if (nav == "Next page")
                    page = Math.Min(page + 1, totalPages - 1);
                else if (nav == "Previous page")
                    page = Math.Max(page - 1, 0);
                else
                    return;
            }
        }

        public static void ShowTransactionDetail(Transaction t)
        {
            var table = new Table().Border(TableBorder.Rounded);
            table.AddColumn("Field");
            table.AddColumn("Value");
            table.AddRow("Transaction Id", t.Id.ToString());
            table.AddRow("Type", t.Transactiontype.ToString());
            table.AddRow("Amount", t.Amount.ToString("C"));
            table.AddRow("Source Account", t.AccountId.ToString());
            table.AddRow("Target Account", t.TargetAccountId?.ToString() ?? "-");
            table.AddRow("Date (UTC)", t.CreatedAt.ToString("u"));
            AnsiConsole.Write(table);
        }

        public static void ShowReceipt(Transaction t, decimal? newBalance = null)
        {
            AnsiConsole.MarkupLine("[green]Operation completed successfully.[/]");
            ShowTransactionDetail(t);
            if (newBalance.HasValue)
                AnsiConsole.MarkupLine($"[bold]Updated balance: {newBalance.Value:C}[/]");
        }

        /// <summary>Prints a friendly message for expected service-layer failures.</summary>
        public static void HandleException(Exception ex)
        {
            switch (ex)
            {
                case ValidationException vex:
                    AnsiConsole.MarkupLine($"[red]Validation failed: {Markup.Escape(vex.Message)}[/]");
                    break;
                case UnauthorizedAccessException uex:
                    AnsiConsole.MarkupLine($"[red]Access denied: {Markup.Escape(uex.Message)}[/]");
                    break;
                case KeyNotFoundException kex:
                    AnsiConsole.MarkupLine($"[red]Not found: {Markup.Escape(kex.Message)}[/]");
                    break;
                case InvalidOperationException ioex:
                    AnsiConsole.MarkupLine($"[red]Cannot complete operation: {Markup.Escape(ioex.Message)}[/]");
                    break;
                case DbUpdateException:
                    AnsiConsole.MarkupLine("[red]Database error: the operation could not be saved. Please try again.[/]");
                    break;
                default:
                    AnsiConsole.MarkupLine("[red]An unexpected error occurred. Please try again.[/]");
                    break;
            }
        }
    }
}
