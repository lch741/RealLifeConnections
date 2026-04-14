namespace backend.DTOs
{
    public class RegisterResponseDto
    {
        public required string Message { get; set; }
        public required string Email { get; set; }
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public string? ProfileImageUrl { get; set; }
        public List<RegisterInterestResultDto> InterestSelections { get; set; } = new();
    }
}
