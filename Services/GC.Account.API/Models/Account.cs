using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GC.Account.API.Models
{
    public class Account
    {
        [Key]
        public int Id { get; set; }

        // Relación con el usuario de autenticación (UserId es la clave foránea)
        [Required]
        public int UserId { get; set; } 

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? Alias { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Propiedad de navegación para las transacciones (Tu historial)
        public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    }
}