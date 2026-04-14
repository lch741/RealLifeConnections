namespace backend.DTOs
{
    public class AuthResponseDto
    {
        public required string Message { get; set; }
        public required string Token { get; set; }
        public required AuthUserDto User { get; set; }
    }
}
