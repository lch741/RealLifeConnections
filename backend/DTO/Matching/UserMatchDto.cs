namespace backend.DTO.Matching
{
    /// <summary>
    /// DTO for user matching recommendations based on personality and preferences.
    /// </summary>
    public class UserMatchDto
    {
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public required string Region { get; set; }
        public required string Suburb { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Culture { get; set; }

        /// <summary>
        /// Compatibility score with current user (0-100).
        /// </summary>
        public int CompatibilityScore { get; set; }

        /// <summary>
        /// Reason for the match (e.g., "High personality compatibility", "Similar interests").
        /// </summary>
        public string? MatchReason { get; set; }

        public List<RegisterInterestResultDto> SharedInterests { get; set; } = new();
        public PersonalityDto? Personality { get; set; }
    }
}
