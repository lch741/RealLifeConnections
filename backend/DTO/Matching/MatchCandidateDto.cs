namespace backend.DTOs
{
    public class MatchCandidateDto
    {
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public required string City { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Culture { get; set; }
        public List<RegisterInterestResultDto> SharedInterests { get; set; } = new();
    }
}
