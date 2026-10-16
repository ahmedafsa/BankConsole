using BankSystem.DAL;
using Microsoft.EntityFrameworkCore;
using MODELS;
using Spectre.Console;

namespace BankSystem.Menus
{
    internal sealed class AdminUserMenu
    {
        private readonly BankDbContext _context;
        private readonly SessionAdminUserServices _users;

        public AdminUserMenu(BankDbContext context, SessionAdminUserServices users)
        {
            _context = context;
            _users = users;
        }

        public void Run()
        {
            while (true)
            {
                AnsiConsole.Clear();
                AnsiConsole.Write(new Rule("[bold]User Management[/]"));
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Choose an action:")
                        .AddChoices(
                            "List users",
                            "Add user",
                            "Delete user",
                            "Change any user's username",
                            "Change or reset any user's password",
                            "Back"));

                try
                {
                    switch (choice)
                    {
                        case "List users": ListUsers(); break;
                        case "Add user": AddUser(); break;
                        case "Delete user": DeleteUser(); break;
                        case "Change any user's username": ChangeUsername(); break;
                        case "Change or reset any user's password": ChangePassword(); break;
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

        private void ListUsers()
        {
            var users = _context.Users.AsNoTracking().OrderBy(u => u.Id).ToList();
            MenuHelpers.ShowUsersTable(users);
            MenuHelpers.Pause();
        }

        private void AddUser()
        {
            string fullName = MenuHelpers.PromptNonEmpty("Full name (min 3 chars):");
            string username = MenuHelpers.PromptNonEmpty("Username (min 3 chars):");
            string password = MenuHelpers.PromptPasswordWithConfirm();
            var roleChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Role:")
                    .AddChoices("Client", "Admin"));
            Userrole role = roleChoice == "Admin" ? Userrole.Admin : Userrole.Client;

            try
            {
                if (_users.AddUser(fullName, username, password, role))
                    AnsiConsole.MarkupLine($"[green]User '{Markup.Escape(username.Trim())}' created.[/]");
                else
                    AnsiConsole.MarkupLine("[red]User could not be created.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void DeleteUser()
        {
            ListUsersQuick();
            int id = MenuHelpers.AskInt("Enter User Id to delete:");
            if (id == _users.CurrentUser.Id)
            {
                AnsiConsole.MarkupLine("[red]You cannot delete your own account.[/]");
                MenuHelpers.Pause();
                return;
            }
            if (!MenuHelpers.ConfirmAction($"Delete user #{id}? This may remove their accounts data per DB rules."))
            {
                AnsiConsole.MarkupLine("[grey]Deletion cancelled.[/]");
                MenuHelpers.Pause();
                return;
            }
            try
            {
                if (_users.DeleteUser(id))
                    AnsiConsole.MarkupLine("[green]User deleted.[/]");
                else
                    AnsiConsole.MarkupLine("[red]User could not be deleted.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ChangeUsername()
        {
            string current = MenuHelpers.PromptNonEmpty("Current username:");
            string next = MenuHelpers.PromptNonEmpty("New username (min 3 chars):");
            try
            {
                if (_users.ChangeUsername(current, next))
                    AnsiConsole.MarkupLine("[green]Username changed.[/]");
                else
                    AnsiConsole.MarkupLine("[red]Username could not be changed.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ChangePassword()
        {
            string target = MenuHelpers.PromptNonEmpty("Target username:");
            bool isSelf = string.Equals(target.Trim(), _users.CurrentUser.Username, StringComparison.Ordinal);
            string currentProof = string.Empty;
            if (isSelf)
            {
                currentProof = MenuHelpers.PromptPassword("Current password:");
            }
            else
            {
                AnsiConsole.MarkupLine("[grey]Resetting another user's password — current password not required.[/]");
            }
            string next = MenuHelpers.PromptPasswordWithConfirm();

            try
            {
                if (_users.ChangePassword(target, currentProof, next))
                    AnsiConsole.MarkupLine($"[green]Password updated for '{Markup.Escape(target.Trim())}'.[/]");
                else
                    AnsiConsole.MarkupLine("[red]Password could not be updated.[/]");
            }
            catch (Exception ex)
            {
                MenuHelpers.HandleException(ex);
            }
            MenuHelpers.Pause();
        }

        private void ListUsersQuick()
        {
            var users = _context.Users.AsNoTracking().OrderBy(u => u.Id).ToList();
            MenuHelpers.ShowUsersTable(users);
        }
    }
}
