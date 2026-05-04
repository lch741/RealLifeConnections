using backend.DTO.Matching;

namespace backend.DTOs
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public required string Region { get; set; }
        public required string Suburb { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationStatus { get; set; } = "pending";
        public bool CanMatch { get; set; }

        // Demographic
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Culture { get; set; }

        // Personality traits
        public PersonalityDto? Personality { get; set; }

        public List<RegisterInterestResultDto> InterestSelections { get; set; } = new();
    }
}
