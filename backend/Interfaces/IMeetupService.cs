using System.Security.Claims;
using backend.DTO.Matching;
using backend.DTOs;

namespace backend.Interfaces
{
    /// <summary>
    /// Service for managing Meetup Events - CRUD operations and business logic.
    /// </summary>
    public interface IMeetupService
    {
        /// <summary>
        /// Create a new meetup event.
        /// </summary>
        Task<MeetupEventDto> CreateMeetupAsync(ClaimsPrincipal principal, CreateMeetupDto dto);

        /// <summary>
        /// Get a meetup event by ID.
        /// </summary>
        Task<MeetupEventDto> GetMeetupAsync(int meetupId);

        /// <summary>
        /// Get all meetups (with optional filtering).
        /// </summary>
        Task<List<MeetupEventDto>> GetAllMeetupsAsync(int? skip = 0, int? take = 20);

        /// <summary>
        /// Get meetups in a specific region and suburb.
        /// </summary>
        Task<List<MeetupEventDto>> GetMeetupsByLocationAsync(string region, string suburb);

        /// <summary>
        /// Get meetups for a specific activity.
        /// </summary>
        Task<List<MeetupEventDto>> GetMeetupsByActivityAsync(int activityId);

        /// <summary>
        /// Get upcoming meetups for the next N days.
        /// </summary>
        Task<List<MeetupEventDto>> GetUpcomingMeetupsAsync(int daysAhead = 30);

        /// <summary>
        /// Update an existing meetup event (creator only).
        /// </summary>
        Task<MeetupEventDto> UpdateMeetupAsync(ClaimsPrincipal principal, int meetupId, UpdateMeetupDto dto);

        /// <summary>
        /// Delete a meetup event (creator only).
        /// </summary>
        Task DeleteMeetupAsync(ClaimsPrincipal principal, int meetupId);

        /// <summary>
        /// Join a meetup event as a participant.
        /// </summary>
        Task<UserMeetupDto> JoinMeetupAsync(ClaimsPrincipal principal, int meetupId);

        /// <summary>
        /// Leave a meetup event.
        /// </summary>
        Task LeaveMeetupAsync(ClaimsPrincipal principal, int meetupId);

        /// <summary>
        /// Get all meetups the current user has joined.
        /// </summary>
        Task<List<MeetupEventDto>> GetUserJoinedMeetupsAsync(ClaimsPrincipal principal);

        /// <summary>
        /// Get all meetups created by the current user.
        /// </summary>
        Task<List<MeetupEventDto>> GetUserCreatedMeetupsAsync(ClaimsPrincipal principal);

        /// <summary>
        /// Get participants of a meetup event.
        /// </summary>
        Task<List<UserMeetupDto>> GetMeetupParticipantsAsync(int meetupId);

        /// <summary>
        /// Check if a user has joined a specific meetup.
        /// </summary>
        Task<bool> IsUserJoinedAsync(ClaimsPrincipal principal, int meetupId);

        /// <summary>
        /// Confirm a participant's attendance for a meetup (creator only).
        /// </summary>
        Task<UserMeetupDto> ConfirmParticipantAsync(ClaimsPrincipal principal, int meetupId, int participantId);

        /// <summary>
        /// Reject or remove a participant from a meetup (creator only).
        /// </summary>
        Task RejectParticipantAsync(ClaimsPrincipal principal, int meetupId, int participantId);

        /// <summary>
        /// Get pending participants awaiting confirmation for a meetup (creator only).
        /// </summary>
        Task<List<UserMeetupDto>> GetPendingParticipantsAsync(int meetupId);

        /// <summary>
        /// Update the status of a meetup event (creator only).
        /// </summary>
        Task<MeetupEventDto> UpdateMeetupStatusAsync(ClaimsPrincipal principal, int meetupId, string status);

        /// <summary>
        /// Suggest a location for a meetup event.
        /// </summary>
        Task<MeetupLocationSuggestionDto> SuggestLocationAsync(ClaimsPrincipal principal, int meetupId, SuggestLocationDto dto);

        /// <summary>
        /// Get location suggestions for a meetup event.
        /// </summary>
        Task<List<MeetupLocationSuggestionDto>> GetLocationSuggestionsAsync(int meetupId);

        /// <summary>
        /// Choose a location for a meetup event (creator only).
        /// </summary>
        Task<MeetupLocationSuggestionDto> ChooseLocationAsync(ClaimsPrincipal principal, int meetupId, int locationSuggestionId);
    }
}
