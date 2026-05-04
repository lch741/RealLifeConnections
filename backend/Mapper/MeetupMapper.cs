using backend.DTO.Meetup;
using backend.Models;

namespace backend.Mapper
{
    public static class MeetupMapper
    {
        public static MeetupEventDto ToMeetupEventDto(MeetupEvent meetup, int currentParticipants = 0)
        {
            return new MeetupEventDto
            {
                Id = meetup.Id,
                Title = meetup.Title,
                Description = meetup.Description,
                Region = meetup.Region,
                Suburb = meetup.Suburb,
                LocationName = meetup.LocationName,
                ActivityId = meetup.ActivityId,
                ActivityName = meetup.Activity?.Name ?? "Unknown Activity",
                EventDate = meetup.EventDate,
                StartTime = meetup.StartTime,
                EndTime = meetup.EndTime,
                MaxParticipants = meetup.MaxParticipants,
                CurrentParticipants = currentParticipants > 0 ? currentParticipants : meetup.Participants.Count,
                Status = meetup.Status.ToString(),
                ConfirmedAt = meetup.ConfirmedAt,
                CompletedAt = meetup.CompletedAt,
                CreatedAt = meetup.CreatedAt,
                UpdatedAt = meetup.UpdatedAt,
                CreatorId = meetup.CreatorId,
                CreatorName = meetup.Creator?.UserName ?? "Unknown",
                Participants = meetup.Participants
                    .Select(up => ToUserMeetupDto(up))
                    .ToList(),
                LocationSuggestions = meetup.LocationSuggestions
                    .Select(ls => ToLocationSuggestionDto(ls))
                    .ToList()
            };
        }

        public static UserMeetupDto ToUserMeetupDto(UserMeetup userMeetup)
        {
            return new UserMeetupDto
            {
                Id = userMeetup.Id,
                UserId = userMeetup.UserId,
                UserName = userMeetup.User?.UserName ?? "Unknown",
                AvatarUrl = userMeetup.User?.ProfileImageUrl,
                Status = userMeetup.Status.ToString(),
                JoinedAt = userMeetup.JoinedAt,
                IsConfirmed = userMeetup.IsConfirmed,
                ConfirmedAt = userMeetup.ConfirmedAt
            };
        }

        public static MeetupLocationSuggestionDto ToLocationSuggestionDto(MeetupLocationSuggestion suggestion)
        {
            return new MeetupLocationSuggestionDto
            {
                Id = suggestion.Id,
                MeetupEventId = suggestion.MeetupEventId,
                SuggestedByUserId = suggestion.SuggestedByUserId,
                SuggestedByUserName = suggestion.SuggestedByUser?.UserName ?? "Unknown",
                Name = suggestion.Name,
                Address = suggestion.Address,
                Type = suggestion.Type.ToString(),
                IsChosen = suggestion.IsChosen,
                CreatedAt = suggestion.CreatedAt
            };
        }

        public static MeetupEvent ToMeetupModel(CreateMeetupDto dto, AppUser creator, Activity activity)
        {
            return new MeetupEvent
            {
                CreatorId = creator.Id,
                Creator = creator,
                Title = dto.Title,
                Description = dto.Description,
                Region = dto.Region,
                Suburb = dto.Suburb,
                LocationName = dto.LocationName,
                ActivityId = dto.ActivityId,
                Activity = activity,
                EventDate = dto.EventDate,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                MaxParticipants = dto.MaxParticipants,
                Status = MeetupStatus.Open,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        public static void ApplyMeetupUpdate(MeetupEvent meetup, UpdateMeetupDto dto)
        {
            if (!string.IsNullOrWhiteSpace(dto.Title))
            {
                meetup.Title = dto.Title.Trim();
            }

            if (dto.Description != null)
            {
                meetup.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Region))
            {
                meetup.Region = dto.Region.Trim();
            }

            if (!string.IsNullOrWhiteSpace(dto.Suburb))
            {
                meetup.Suburb = dto.Suburb.Trim();
            }

            if (dto.LocationName != null)
            {
                meetup.LocationName = string.IsNullOrWhiteSpace(dto.LocationName) ? null : dto.LocationName.Trim();
            }

            if (dto.ActivityId.HasValue)
            {
                meetup.ActivityId = dto.ActivityId.Value;
            }

            if (dto.EventDate.HasValue)
            {
                meetup.EventDate = dto.EventDate.Value;
            }

            if (dto.StartTime.HasValue)
            {
                meetup.StartTime = dto.StartTime.Value;
            }

            if (dto.EndTime.HasValue)
            {
                meetup.EndTime = dto.EndTime.Value;
            }

            if (dto.MaxParticipants.HasValue)
            {
                meetup.MaxParticipants = dto.MaxParticipants.Value;
            }

            meetup.UpdatedAt = DateTime.UtcNow;
        }

        public static MeetupLocationSuggestion ToLocationSuggestion(
            int meetupId,
            int suggestedByUserId,
            SuggestLocationDto dto)
        {
            return new MeetupLocationSuggestion
            {
                MeetupEventId = meetupId,
                SuggestedByUserId = suggestedByUserId,
                Name = dto.Name,
                Address = dto.Address,
                Type = Enum.TryParse<MeetupLocationType>(dto.Type, true, out var type)
                    ? type
                    : MeetupLocationType.Custom,
                IsChosen = false,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}
