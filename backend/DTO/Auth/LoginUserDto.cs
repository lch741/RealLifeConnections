using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class LoginUserDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        public required string Password { get; set; }
    }
}
