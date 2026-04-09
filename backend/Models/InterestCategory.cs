using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class InterestCategory
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; } // sports, art, etc.

        public List<UserInterest> UserInterests { get; set; } = new();
    }
}