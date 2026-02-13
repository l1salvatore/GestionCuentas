using System.ComponentModel.DataAnnotations;

namespace GC.Auth.API.Inputs
{
    public class RegisterInput
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}