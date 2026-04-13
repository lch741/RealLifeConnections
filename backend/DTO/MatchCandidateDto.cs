namespace backend.DTOs
{
    public class MatchCandidateDto
    {
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public List<RegisterInterestResultDto> SharedInterests { get; set; } = new();
    }
}
