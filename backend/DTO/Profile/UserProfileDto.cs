namespace backend.DTOs
{
    public class UserProfileDto
    {
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsVerified { get; set; }
        public string VerificationStatus { get; set; } = "pending";
        public bool CanMatch { get; set; }
        public List<RegisterInterestResultDto> InterestSelections { get; set; } = new();
    }
}
