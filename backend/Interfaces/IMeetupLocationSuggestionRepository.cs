using backend.Models;

namespace backend.Interfaces
{
    /// <summary>
    /// Repository interface for MeetupLocationSuggestion data access operations.
    /// </summary>
    public interface IMeetupLocationSuggestionRepository
    {
        /// <summary>
        /// Get a location suggestion by ID.
        /// </summary>
        Task<MeetupLocationSuggestion> GetByIdAsync(int id);

        /// <summary>
        /// Get all location suggestions for a specific meetup.
        /// </summary>
        Task<List<MeetupLocationSuggestion>> GetByMeetupAsync(int meetupId);

        /// <summary>
        /// Get location suggestions made by a specific user.
        /// </summary>
        Task<List<MeetupLocationSuggestion>> GetBySuggesterAsync(int userId);

        /// <summary>
        /// Get all chosen locations (IsChosen = true).
        /// </summary>
        Task<List<MeetupLocationSuggestion>> GetChosenLocationsAsync();

        /// <summary>
        /// Get the chosen location for a specific meetup.
        /// </summary>
        Task<MeetupLocationSuggestion> GetChosenLocationForMeetupAsync(int meetupId);

        /// <summary>
        /// Create a new location suggestion.
        /// </summary>
        Task<MeetupLocationSuggestion> CreateAsync(MeetupLocationSuggestion suggestion);

        /// <summary>
        /// Update a location suggestion.
        /// </summary>
        Task<MeetupLocationSuggestion> UpdateAsync(MeetupLocationSuggestion suggestion);

        /// <summary>
        /// Delete a location suggestion.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Mark a location as chosen (and unmark others for the same meetup).
        /// </summary>
        Task<MeetupLocationSuggestion> MarkAsChosenAsync(int locationSuggestionId);

        /// <summary>
        /// Check if a user has already suggested a location for a meetup.
        /// </summary>
        Task<bool> HasUserSuggestedAsync(int userId, int meetupId);

        /// <summary>
        /// Get location suggestions by type (Cafe, Park, etc.).
        /// </summary>
        Task<List<MeetupLocationSuggestion>> GetByTypeAsync(MeetupLocationType type);
    }
}
