using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.Models;
using backend.DTOs;
using backend.DTO.Matching;

namespace backend.DTO.Matching
{
    public class SearchingCandidateDto
    {
        public int UserId { get; set; }
        public required string UserName { get; set; }
        public string? Bio { get; set; }
        public required string Region { get; set; }
        public required string Suburb { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Gender { get; set; }
        public int? Age { get; set; }
        public string? Culture { get; set; }

        /// <summary>
        /// Compatibility score with searcher (0-100).
        /// </summary>
        public int CompatibilityScore { get; set; }

        public List<RegisterInterestResultDto> Interests { get; set; } = new();
        public PersonalityDto? Personality { get; set; }
    }
}