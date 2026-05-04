using System.ComponentModel.DataAnnotations;
using backend.DTO.Matching;

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

        [Required]
        [MaxLength(50)]
        public required string Region { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Suburb { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        public string? ProfileImageUrl { get; set; }

        // Demographic info
        [MaxLength(20)]
        public string? Gender { get; set; }

        public int? Age { get; set; }

        [MaxLength(100)]
        public string? Culture { get; set; }

        // Personality traits (optional on registration)
        public PersonalityDto? Personality { get; set; }

        // Preferred distance for meetups
        public int? PreferredDistanceKm { get; set; }

        [Required]
        public List<RegisterInterestSelectionDto> InterestSelections { get; set; } = new();
    }
}
