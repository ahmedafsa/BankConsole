using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MODELS
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        public int AccountId { get; set; }

        [ForeignKey(nameof(AccountId))]
        [InverseProperty(nameof(Account.Transactions))]
        public Account Account { get; set; } = null!;

        [Range(0.01, 999999999)]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        public Transactiontype Transactiontype { get; set; }

        public int? TargetAccountId { get; set; }

        [ForeignKey(nameof(TargetAccountId))]
        public Account? TargetAccount { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}