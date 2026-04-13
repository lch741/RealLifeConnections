using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class FaceVerificationRequestDto
    {
        [Required]
        [Url]
        public required string LiveCaptureUrl { get; set; }
    }
}
