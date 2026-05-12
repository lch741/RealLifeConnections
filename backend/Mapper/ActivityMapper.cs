using backend.Models;
using backend.DTO;

namespace backend.Mapper
{
    public static class ActivityMapper
    {
        public static ActivityDto ToActivityDto(Activity activity)
        {
            return new ActivityDto
            {
                Id = activity.Id,
                Name = activity.Name,
                Description = activity.Description,
                Type = activity.Type.ToString()
            };
        }

        public static Activity ToActivityModel(
            string name,
            string? description = null,
            ActivityType type = ActivityType.Custom)
        {
            return new Activity
            {
                Name = name,
                Description = description,
                Type = type
            };
        }

        public static void ApplyActivityUpdate(Activity activity, string? name, string? description)
        {
            if (!string.IsNullOrWhiteSpace(name))
            {
                activity.Name = name.Trim();
            }

            if (description != null)
            {
                activity.Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
            }
        }
    }
}
