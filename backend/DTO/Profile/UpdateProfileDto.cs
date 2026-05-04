using System.ComponentModel.DataAnnotations;
using backend.DTO.Matching;

namespace backend.DTOs
{
    public class UpdateProfileDto
    {
        [MaxLength(30)]
        public string? UserName { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        [MaxLength(50)]
        public string? Region { get; set; }

        [MaxLength(50)]
        public string? Suburb { get; set; }

        // Demographic updatable fields
        [MaxLength(20)]
        public string? Gender { get; set; }

        public int? Age { get; set; }

        [MaxLength(100)]
        public string? Culture { get; set; }

        // Personality traits
        public PersonalityDto? Personality { get; set; }

        // Preferred distance for meetups
        public int? PreferredDistanceKm { get; set; }

        public List<RegisterInterestSelectionDto> InterestSelections { get; set; } = new();
    }
}
