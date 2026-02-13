using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GC.Account.API.Enums;

namespace GC.Account.API.Models
{
    public class Transaction
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int AccountId { get; set; }

        // Propiedad de navegación (Relación con Account)
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; } = null!;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; }

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
    }

}