using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UpdateProfileDto
    {
        [MaxLength(30)]
        public string? UserName { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        [Required]
        public required string City { get; set; }

        // Demographic updatable fields
        [MaxLength(20)]
        public string? Gender { get; set; }

        public int? Age { get; set; }

        [MaxLength(100)]
        public string? Culture { get; set; }

        public List<RegisterInterestSelectionDto> InterestSelections { get; set; } = new();
    }
}
