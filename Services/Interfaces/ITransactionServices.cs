using System;
using System.Collections.Generic;
using System.Text;
using MODELS;
namespace BankSystem.Services.Interfaces
{
    public interface ITransactionServices
    {
        List<Transaction> ShowAllTransactions();

        List<Transaction> ShowTransactionsByAccountId(int accountId);
        Transaction? ShowTransactionById(int transactionId);

        Transaction Deposit(int accountId, decimal amount);
        Transaction Withdraw(int accountId, decimal amount);
        Transaction Transfer(int accountId, int targetAccountId, decimal amount);

        List<Transaction> ShowTransfers(int? accountId = null);
        List<Transaction> ShowWithdraws(int? accountId = null);
        List<Transaction> ShowDeposits(int? accountId = null);
    }
}
