using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class RegisterUserDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        [Required]
        [MaxLength(30)]
        public required string UserName { get; set; }

        [Required]
        [MinLength(6)]
        public required string Password { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        public string? ProfileImageUrl { get; set; }

        [Required]
        public List<RegisterInterestSelectionDto> InterestSelections { get; set; } = new();
    }
}
