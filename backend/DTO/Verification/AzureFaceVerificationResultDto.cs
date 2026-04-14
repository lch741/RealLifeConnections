namespace backend.DTOs
{
    public class AzureFaceVerificationResultDto
    {
        public bool IsIdentical { get; set; }
        public double Confidence { get; set; }
        public string Provider { get; set; } = "AzureFace";
    }
}
