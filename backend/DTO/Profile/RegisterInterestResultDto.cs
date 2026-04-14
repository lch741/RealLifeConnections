namespace backend.DTOs
{
    public class RegisterInterestResultDto
    {
        public int CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public List<string> Interests { get; set; } = new();
    }
}
