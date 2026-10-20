# 🏦 BankSystem

A console banking app in C# and .NET with role-based access. Log in as an Admin to manage users, accounts, and transactions, or as a Client to manage your own accounts and money.

Built with Entity Framework Core and SQL Server, with an interactive terminal UI powered by Spectre.Console.

Developed as a team mini project for **DEPI (Digital Egypt Pioneers Initiative)**.

## Features

| Area                    | What you can do                                                                                                          |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| **Login**               | Masked password input, BCrypt-hashed passwords, automatic routing to the Admin or Client menu by role                    |
| **Admin: Users**        | List, add (with role), and delete users. Change usernames. Change or reset passwords                                     |
| **Admin: Accounts**     | Create, delete, and list accounts. Find accounts by username                                                             |
| **Admin: Transactions** | Deposit, withdraw, and transfer between any accounts. View and filter all transactions. Look up by ID                    |
| **Client**              | View your accounts, deposit, withdraw, transfer, view statements and filtered history, change your username and password |

## Architecture

The project is split into three layers:

| Layer              | Folder            | Responsibility                                                       |
| ------------------ | ----------------- | -------------------------------------------------------------------- |
| **DAL and Models** | `DAL/`, `Models/` | EF Core `DbContext` and entities (`User`, `Account`, `Transaction`)  |
| **Services**       | `Services/`       | Business logic, validation, and authorization, defined by interfaces |
| **Menus**          | `Menus/`          | Console UI, prompts, tables, and navigation                          |

```
BankSystem/
├── Program.cs        # Entry point: migrate DB, seed admin, start login
├── DAL/              # BankDbContext
├── Models/           # User, Account, Transaction, enums
├── Services/
│   ├── Interfaces/
│   └── Classes/      # Auth, admin, client, and transaction services
├── Menus/            # Login, Admin, and Client menus
└── Migrations/       # EF Core migrations
```

Authorization is enforced inside the services, not just the menus. Admin-only operations throw `UnauthorizedAccessException` when called by a client.

## Tech stack

- .NET 10 (C#)
- Entity Framework Core 10 with SQL Server
- Spectre.Console
- BCrypt.Net-Next

## Getting started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB, Express, or full)

### Run

```bash
git clone https://github.com/ahmedafsa/BankConsole.git
cd BankSystem
dotnet run
```

On startup the app applies migrations and creates the database automatically.

### Database connection

The connection string is set in `DAL/BankDbContext.cs`. The default is:

```
Server=localhost;Database=BankDB;Integrated Security=True;TrustServerCertificate=True;
```

Change it there if your SQL Server instance is named differently.

## Demo account

This is a demo Admin account created on first run.

| Username | Password    |
| -------- | ----------- |
| `admin`  | `admin@123` |
