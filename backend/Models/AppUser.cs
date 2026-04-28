using System.ComponentModel.DataAnnotations;

namespace api.Models
{
    public enum Gender
    {
        Male = 0,
        Female = 1,
        Other = 2,
        NotToTell = 3
    }

    public class AppUser
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public required string Email { get; set; }

        // Username (unique, 3-30 chars)
        [Required]
        [MaxLength(30)]
        public required string UserName { get; set; }

        // Password hash )
        [Required]
        public required string PasswordHash { get; set; }

        // City (default "online")
        public string City { get; set; } = "online";

        // Description
        [MaxLength(300)]
        public string? Bio { get; set; }

        // Profile image URL
        public string? ProfileImageUrl { get; set; }

        // Verification status
        public bool IsVerified { get; set; } = false;

        // List of user interests
        public List<UserInterest> Interests { get; set; } = new();

        // Verification records
        public List<Verification> Verifications { get; set; } = new();

        // Demographic fields
        public Gender Gender { get; set; } = Gender.NotToTell;

        // Age (nullable — can be omitted)
        public int? Age { get; set; }

        // Culture / nationality (e.g., "Canadian", "Japanese")
        [MaxLength(100)]
        public string? Culture { get; set; }
    }
}