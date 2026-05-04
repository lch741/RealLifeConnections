using System.Security.Claims;
using backend.DTO.Feedback;
using backend.DTO.Matching;

namespace backend.Interfaces
{
    /// <summary>
    /// Service for managing Meetup Feedback - user reviews and ratings after meetups.
    /// </summary>
    public interface IMeetupFeedbackService
    {
        /// <summary>
        /// Submit feedback for a completed meetup.
        /// </summary>
        Task<MeetupFeedbackDto> SubmitFeedbackAsync(ClaimsPrincipal principal, int meetupId, MeetupFeedbackRequestDto dto);

        /// <summary>
        /// Get feedback for a specific meetup.
        /// </summary>
        Task<List<MeetupFeedbackDto>> GetMeetupFeedbackAsync(int meetupId);

        /// <summary>
        /// Get feedback submitted by a specific user.
        /// </summary>
        Task<List<MeetupFeedbackDto>> GetUserFeedbackAsync(int userId);

        /// <summary>
        /// Get feedback about a specific user (received from others).
        /// </summary>
        Task<List<MeetupFeedbackDto>> GetFeedbackAboutUserAsync(int userId);

        /// <summary>
        /// Get average rating for a specific user.
        /// </summary>
        Task<double> GetUserAverageRatingAsync(int userId);

        /// <summary>
        /// Update feedback (submitter only).
        /// </summary>
        Task<MeetupFeedbackDto> UpdateFeedbackAsync(ClaimsPrincipal principal, int feedbackId, MeetupFeedbackRequestDto dto);

        /// <summary>
        /// Delete feedback (submitter only).
        /// </summary>
        Task DeleteFeedbackAsync(ClaimsPrincipal principal, int feedbackId);

        /// <summary>
        /// Check if user has already submitted feedback for a meetup.
        /// </summary>
        Task<bool> HasSubmittedFeedbackAsync(int userId, int meetupId);

        /// <summary>
        /// Get feedback statistics for a user (average rating, total feedbacks, etc.).
        /// </summary>
        Task<UserFeedbackStatsDto> GetUserFeedbackStatsAsync(int userId);
    }
}
