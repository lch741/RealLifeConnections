using backend.DTO.Matching;
using backend.Models;

namespace backend.Interfaces
{
    /// <summary>
    /// Service for matching users based on personality traits, location, activities, and time preferences.
    /// </summary>
    public interface IMeetupMatchingService
    {
        /// <summary>
        /// Get personalized meetup/event recommendations for a user based on their personality traits and preferences.
        /// </summary>
        Task<List<MeetupMatchDto>> GetRecommendedMeetupsAsync(int userId, int limit = 10);

        /// <summary>
        /// Find compatible users for a user to meet up with based on personality traits, location, and preferences.
        /// </summary>
        Task<List<UserMatchDto>> FindCompatibleUsersAsync(int userId, int limit = 10);

        /// <summary>
        /// Calculate personality compatibility between two users (0-100 scale).
        /// </summary>
        Task<int> CalculatePersonalityCompatibilityAsync(int userId1, int userId2);

        /// <summary>
        /// Get detailed compatibility breakdown between two users (individual trait scores).
        /// </summary>
        Task<PersonalityCompatibilityDto> GetDetailedCompatibilityAsync(int userId1, int userId2);

        /// <summary>
        /// Score a meetup event for a specific user based on their preferences and personality.
        /// </summary>
        Task<int> ScoreMeetupForUserAsync(int userId, int meetupEventId);

        /// <summary>
        /// Find matching meetups based on user's location, interests, and personality.
        /// </summary>
        Task<List<MeetupMatchDto>> FindMeetupsAsync(int userId, string region, string suburb, int radiusKm = 50);

        /// <summary>
        /// Check if a user's time preferences match a meetup's scheduled time.
        /// </summary>
        bool IsTimePreferenceMatch(AppUser user, MeetupEvent meetup);

        /// <summary>
        /// Get personality trait summary for a user (normalized labels instead of numeric values).
        /// </summary>
        Task<PersonalityTraitsSummaryDto> GetPersonalityTraitsSummaryAsync(int userId);
    }
}
