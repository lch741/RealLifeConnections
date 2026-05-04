using backend.Models;

namespace backend.Interfaces
{
    /// <summary>
    /// Repository interface for MeetupEvent data access operations.
    /// </summary>
    public interface IMeetupRepository
    {
        /// <summary>
        /// Get a meetup event by ID.
        /// </summary>
        Task<MeetupEvent> GetByIdAsync(int id);

        /// <summary>
        /// Get all active meetup events.
        /// </summary>
        Task<List<MeetupEvent>> GetAllActiveAsync();

        /// <summary>
        /// Get meetups by region and suburb (for location-based filtering).
        /// </summary>
        Task<List<MeetupEvent>> GetByLocationAsync(string region, string suburb);

        /// <summary>
        /// Get meetups by activity ID.
        /// </summary>
        Task<List<MeetupEvent>> GetByActivityAsync(int activityId);

        /// <summary>
        /// Get meetups created by a specific user.
        /// </summary>
        Task<List<MeetupEvent>> GetByCreatorAsync(int creatorId);

        /// <summary>
        /// Get meetups within a date range.
        /// </summary>
        Task<List<MeetupEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Create a new meetup event.
        /// </summary>
        Task<MeetupEvent> CreateAsync(MeetupEvent meetup);

        /// <summary>
        /// Update an existing meetup event.
        /// </summary>
        Task<MeetupEvent> UpdateAsync(MeetupEvent meetup);

        /// <summary>
        /// Delete a meetup event.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Get meetups with specific status.
        /// </summary>
        Task<List<MeetupEvent>> GetByStatusAsync(MeetupStatus status);

        /// <summary>
        /// Get upcoming meetups for a specific date range.
        /// </summary>
        Task<List<MeetupEvent>> GetUpcomingAsync(DateTime fromDate, DateTime toDate);

        /// <summary>
        /// Check if a user has joined a specific meetup.
        /// </summary>
        Task<bool> HasUserJoinedAsync(int userId, int meetupId);

        /// <summary>
        /// Get meetups that a user has joined.
        /// </summary>
        Task<List<MeetupEvent>> GetUserJoinedMeetupsAsync(int userId);

        /// <summary>
        /// Get participants for a meetup with optional status filtering.
        /// </summary>
        Task<List<UserMeetup>> GetMeetupParticipantsAsync(int meetupId, string? status = null);

        /// <summary>
        /// Confirm a participant's attendance (update status to confirmed).
        /// </summary>
        Task<UserMeetup> ConfirmParticipantAsync(int meetupId, int userId);

        /// <summary>
        /// Reject or remove a participant from a meetup.
        /// </summary>
        Task RemoveParticipantAsync(int meetupId, int userId);
    }
}
