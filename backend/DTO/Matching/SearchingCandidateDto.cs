using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.DTOs;

namespace backend.DTO.Matching
{
    public class SearchingCandidateDto
    {
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public required string City { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Culture { get; set; }
        public List<RegisterInterestResultDto> Interests { get; set; } = new();
    }
}