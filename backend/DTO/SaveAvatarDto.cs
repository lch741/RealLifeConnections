using System.ComponentModel.DataAnnotations;

namespace backend.DTOs
{
    public class SaveAvatarDto
    {
        [Required]
        [Url]
        public required string AvatarUrl { get; set; }
    }
}
