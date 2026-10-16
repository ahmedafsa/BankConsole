using BankSystem.DAL;
using BankSystem.Menus;
using BankSystem.Services.Classes;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace BankSystem
{
    internal class Program
    {
        static void Main(string[] args)
        {
            try
            {
                
                using var db = new BankDbContext();
                try
                {
                    db.Database.Migrate();
                    db.EnsureAdminSeeded();
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("[yellow]Warning: database setup did not complete. The app will continue, but actions may fail.[/]");
                    AnsiConsole.MarkupLine($"[grey]{Spectre.Console.Markup.Escape(ex.Message)}[/]");
                }

                var auth = new AuthenticationServices(db);
                var login = new LoginMenu(db, auth);
                login.Run();

                AnsiConsole.MarkupLine("[grey]Goodbye![/]");
            }
            catch (Exception)
            {
                AnsiConsole.MarkupLine("[red]The application encountered an unexpected error and had to stop.[/]");
            }
        }
    }
}
