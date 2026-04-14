using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class RegisterInterestSelectionDto
    {
        [Range(1, 8)]
        public int CategoryId { get; set; }

        public List<string> Interests { get; set; } = new();
    }
}
