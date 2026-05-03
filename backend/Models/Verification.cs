using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class Verification
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; }

        // Camera selfie
        [Required]
        public required string ImageUrl { get; set; }

        // Status: pending, approved, rejected
        [Required]
        public required string Status { get; set; } = "approved";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}