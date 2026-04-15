using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace backend.Helper
{
    public static class MatchHelper
    {
        private const double MatchThreshold = 0.8;

        public static string NormalizeInterest(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return string.Empty;

            var key = new string(value
                .Where(char.IsLetterOrDigit)
                .Select(char.ToLowerInvariant)
                .ToArray());

            // optional simple singular
            if (key.Length > 3 && key.EndsWith("s", StringComparison.Ordinal))
            {
                key = key[..^1];
            }

            return key;
        }

        public static bool IsRoughMatch(string left, string right, double threshold = MatchThreshold)
        {
            if (string.IsNullOrEmpty(left) || string.IsNullOrEmpty(right)) return false;

            var a = NormalizeInterest(left);
            var b = NormalizeInterest(right);
            if (a.Length == 0 || b.Length == 0) return false;

            var shorter = a.Length <= b.Length ? a : b;
            var longer  = a.Length > b.Length ? a : b;

            if (!longer.Contains(shorter, StringComparison.Ordinal)) return false;

            var score = (double)shorter.Length / longer.Length;
            return score >= threshold;
        }
        
    }
}