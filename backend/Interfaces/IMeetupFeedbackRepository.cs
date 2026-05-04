using backend.Models;

namespace backend.Interfaces
{
    /// <summary>
    /// Repository interface for MeetupFeedback data access operations.
    /// </summary>
    public interface IMeetupFeedbackRepository
    {
        /// <summary>
        /// Get feedback by ID.
        /// </summary>
        Task<MeetupFeedback> GetByIdAsync(int id);

        /// <summary>
        /// Get all feedback for a specific meetup.
        /// </summary>
        Task<List<MeetupFeedback>> GetByMeetupAsync(int meetupId);

        /// <summary>
        /// Get feedback submitted by a specific user.
        /// </summary>
        Task<List<MeetupFeedback>> GetBySubmitterAsync(int submitterId);

        /// <summary>
        /// Get feedback about a specific user (received from others).
        /// </summary>
        Task<List<MeetupFeedback>> GetAboutUserAsync(int userId);

        /// <summary>
        /// Create new feedback.
        /// </summary>
        Task<MeetupFeedback> CreateAsync(MeetupFeedback feedback);

        /// <summary>
        /// Update existing feedback.
        /// </summary>
        Task<MeetupFeedback> UpdateAsync(MeetupFeedback feedback);

        /// <summary>
        /// Delete feedback.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Check if user has submitted feedback for a specific meetup.
        /// </summary>
        Task<bool> HasFeedbackAsync(int submitterId, int meetupId);

        /// <summary>
        /// Get average rating for a user.
        /// </summary>
        Task<double> GetAverageRatingAsync(int userId);

        /// <summary>
        /// Get feedback count for a user.
        /// </summary>
        Task<int> GetFeedbackCountAsync(int userId);

        /// <summary>
        /// Get recent feedback for a user (ordered by date descending).
        /// </summary>
        Task<List<MeetupFeedback>> GetRecentFeedbackAsync(int userId, int limit = 10);
    }
}
