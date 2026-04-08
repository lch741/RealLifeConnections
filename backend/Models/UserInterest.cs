using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public class UserInterest
    {
        public int Id { get; set; }
 
        public int UserId { get; set; }
        public required AppUser User { get; set; }

        public int CategoryId { get; set; }
        public required InterestCategory Category { get; set; }

        // Subcategory (e.g. "oil painting" under "art")
        [Required]
        [MaxLength(50)]
        [MinLength(2)]
        public required string SubCategory { get; set; }
    }
}