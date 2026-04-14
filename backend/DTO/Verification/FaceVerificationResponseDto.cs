namespace backend.DTOs
{
    public class FaceVerificationResponseDto
    {
        public required string Status { get; set; }
        public required string Message { get; set; }
        public bool IsVerified { get; set; }
        public double Confidence { get; set; }
    }
}
