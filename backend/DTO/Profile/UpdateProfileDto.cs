using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class UpdateProfileDto
    {
        [MaxLength(30)]
        public string? UserName { get; set; }

        [MaxLength(300)]
        public string? Bio { get; set; }

        public List<RegisterInterestSelectionDto> InterestSelections { get; set; } = new();
    }
}
