using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using api.Models;

namespace backend.DTO.Matching
{
    public class SearchingCandidateDto
    {
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public required string City { get; set; }
        public string? AvatarUrl { get; set; }
        public List<UserInterest> Interests { get; set; } = new();
    }
}