namespace backend.DTOs
{
    public class AuthUserDto
    {
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public required string City { get; set; }
        public string? ProfileImageUrl { get; set; }
        // Demographic
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Culture { get; set; }
        public List<RegisterInterestResultDto> InterestSelections { get; set; } = new();
    }
}
