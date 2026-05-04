using backend.Models;

namespace backend.Interfaces
{
    /// <summary>
    /// Repository interface for Activity data access operations.
    /// </summary>
    public interface IActivityRepository
    {
        /// <summary>
        /// Get an activity by ID.
        /// </summary>
        Task<Activity> GetByIdAsync(int id);

        /// <summary>
        /// Get all activities.
        /// </summary>
        Task<List<Activity>> GetAllAsync();

        /// <summary>
        /// Get activities by category.
        /// </summary>
        Task<List<Activity>> GetByCategoryAsync(string category);

        /// <summary>
        /// Create a new activity.
        /// </summary>
        Task<Activity> CreateAsync(Activity activity);

        /// <summary>
        /// Update an existing activity.
        /// </summary>
        Task<Activity> UpdateAsync(Activity activity);

        /// <summary>
        /// Delete an activity.
        /// </summary>
        Task DeleteAsync(int id);

        /// <summary>
        /// Search activities by name.
        /// </summary>
        Task<List<Activity>> SearchByNameAsync(string searchTerm);
    }
}
