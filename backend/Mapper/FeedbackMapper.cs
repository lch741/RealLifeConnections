using backend.DTO.Feedback;
using backend.Models;

namespace backend.Mapper
{
    public static class FeedbackMapper
    {
        public static MeetupFeedbackDto ToFeedbackDto(MeetupFeedback feedback)
        {
            return new MeetupFeedbackDto
            {
                Id = feedback.Id,
                MeetupEventId = feedback.MeetupEventId,
                MeetupTitle = feedback.MeetupEvent?.Title ?? "Unknown Meetup",
                SubmittedByUserId = feedback.UserId,
                SubmittedByUserName = feedback.User?.UserName ?? "Unknown",
                Result = feedback.Result.ToString(),
                Comment = feedback.Comment,
                SubmittedAt = feedback.SubmittedAt
            };
        }

        public static MeetupFeedback ToFeedbackModel(
            int meetupId,
            int userId,
            MeetupFeedbackRequestDto dto)
        {
            return new MeetupFeedback
            {
                MeetupEventId = meetupId,
                UserId = userId,
                Result = Enum.TryParse<MeetupFeedbackResult>(dto.Result, true, out var result)
                    ? result
                    : MeetupFeedbackResult.Okay,
                Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim(),
                SubmittedAt = DateTime.UtcNow
            };
        }

        public static void ApplyFeedbackUpdate(MeetupFeedback feedback, MeetupFeedbackRequestDto dto)
        {
            feedback.Result = Enum.TryParse<MeetupFeedbackResult>(dto.Result, true, out var result)
                ? result
                : MeetupFeedbackResult.Okay;
            feedback.Comment = string.IsNullOrWhiteSpace(dto.Comment) ? null : dto.Comment.Trim();
            feedback.SubmittedAt = DateTime.UtcNow;
        }

        public static UserFeedbackStatsDto ToUserFeedbackStatsDto(
            AppUser user,
            List<MeetupFeedback> feedbacks,
            double averageRating,
            int totalFeedbacks)
        {
            var yesFeedbackCount = feedbacks.Count(f => f.Result == MeetupFeedbackResult.Yes);
            var yesPercentage = totalFeedbacks > 0
                ? Math.Round((double)yesFeedbackCount / totalFeedbacks * 100, 2)
                : 0;

            return new UserFeedbackStatsDto
            {
                UserId = user.Id,
                UserName = user.UserName,
                AverageRating = Math.Round(averageRating, 2),
                TotalFeedbacks = totalFeedbacks,
                WouldMeetAgainPercentage = yesPercentage,
                PositiveTrend = ExtractPositiveTrend(feedbacks),
                SuggestionTrend = ExtractSuggestionTrend(feedbacks),
                RecentFeedbacks = feedbacks
                    .OrderByDescending(f => f.SubmittedAt)
                    .Take(5)
                    .Select(ToFeedbackDto)
                    .ToList()
            };
        }

        private static string? ExtractPositiveTrend(List<MeetupFeedback> feedbacks)
        {
            if (feedbacks.Count == 0)
                return null;

            // Find the most common positive feedback theme
            var positiveFeedbacks = feedbacks
                .Where(f => f.Result == MeetupFeedbackResult.Yes && !string.IsNullOrWhiteSpace(f.Comment))
                .Select(f => f.Comment)
                .ToList();

            if (positiveFeedbacks.Count == 0)
                return null;

            // Simple approach: return a summary of positive ratings
            return $"Positive feedback from {positiveFeedbacks.Count} user(s)";
        }

        private static string? ExtractSuggestionTrend(List<MeetupFeedback> feedbacks)
        {
            if (feedbacks.Count == 0)
                return null;

            // Find suggestions (negative feedback with comments)
            var suggestions = feedbacks
                .Where(f => (f.Result == MeetupFeedbackResult.No || f.Result == MeetupFeedbackResult.Okay) && !string.IsNullOrWhiteSpace(f.Comment))
                .Select(f => f.Comment)
                .ToList();

            if (suggestions.Count == 0)
                return null;

            // Simple approach: indicate that there are suggestions
            return $"{suggestions.Count} user(s) provided feedback for improvement";
        }
    }
}
