using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using BankSystem.DAL;
using BankSystem.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using MODELS;

namespace BankSystem.Services.Classes
{
    public class TransactionServices : ITransactionServices
    {
        private readonly BankDbContext _context;

        public TransactionServices(BankDbContext context)
        {
            _context = context;
        }

        public List<Transaction> ShowAllTransactions()
        {
            return _context.Transactions
                .Include(t => t.Account).ThenInclude(a => a.User)
                .Include(t => t.TargetAccount).ThenInclude(a => a!.User)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public List<Transaction> ShowTransactionsByAccountId(int accountId)
        {
            return _context.Transactions
                .Where(t => t.AccountId == accountId || t.TargetAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public Transaction? ShowTransactionById(int transactionId)
        {
            return _context.Transactions
                .FirstOrDefault(t => t.Id == transactionId);
        }

        public List<Transaction> ShowDeposits(int? accountId = null)
        {
            return _context.Transactions
                .Where(t => t.Transactiontype == Transactiontype.Deposit)
                .Where(t => accountId == null || t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public List<Transaction> ShowWithdraws(int? accountId = null)
        {
            return _context.Transactions
                .Where(t => t.Transactiontype == Transactiontype.Withdraw)
                .Where(t => accountId == null || t.AccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }

        public List<Transaction> ShowTransfers(int? accountId = null)
        {
            return _context.Transactions
                .Include(t => t.Account).ThenInclude(a => a.User)
                .Include(t => t.TargetAccount).ThenInclude(a => a!.User)
                .Where(t => t.Transactiontype == Transactiontype.Transfer)
                .Where(t => accountId == null || t.AccountId == accountId || t.TargetAccountId == accountId)
                .OrderByDescending(t => t.CreatedAt)
                .ToList();
        }


        public Transaction Deposit(int accountId, decimal amount)
        {
            var transaction = new Transaction
            {
                AccountId = accountId,
                Amount = amount,
                Transactiontype = Transactiontype.Deposit
            };

            Validator.ValidateObject(transaction, new ValidationContext(transaction), validateAllProperties: true);

            var account = _context.Accounts.Find(accountId)
                ?? throw new KeyNotFoundException($"Account {accountId} not found.");

            account.Balance += amount;

            _context.Transactions.Add(transaction);
            _context.SaveChanges(); 

            return transaction;
        }

        public Transaction Withdraw(int accountId, decimal amount)
        {
            var transaction = new Transaction
            {
                AccountId = accountId,
                Amount = amount,
                Transactiontype = Transactiontype.Withdraw
            };

            Validator.ValidateObject(transaction, new ValidationContext(transaction), validateAllProperties: true);

            var account = _context.Accounts.Find(accountId)
                ?? throw new KeyNotFoundException($"Account {accountId} not found.");

            if (account.Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            account.Balance -= amount;

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return transaction;
        }

        public Transaction Transfer(int accountId, int targetAccountId, decimal amount)
        {
            if (accountId == targetAccountId)
                throw new InvalidOperationException("Cannot transfer to the same account.");

            var transaction = new Transaction
            {
                AccountId = accountId,
                TargetAccountId = targetAccountId,
                Amount = amount,
                Transactiontype = Transactiontype.Transfer
            };

            Validator.ValidateObject(transaction, new ValidationContext(transaction), validateAllProperties: true);

            var source = _context.Accounts.Find(accountId)
                ?? throw new KeyNotFoundException($"Account {accountId} not found.");

            var target = _context.Accounts.Find(targetAccountId)
                ?? throw new KeyNotFoundException($"Account {targetAccountId} not found.");

            if (source.Balance < amount)
                throw new InvalidOperationException("Insufficient balance.");

            source.Balance -= amount;
            target.Balance += amount;

            _context.Transactions.Add(transaction);
            _context.SaveChanges();

            return transaction;
        }
    }
}