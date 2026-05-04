namespace backend.DTO
{
    /// <summary>
    /// DTO for Activity response.
    /// </summary>
    public class ActivityDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
