using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.Data;
using backend.DTO.Meetup;
using backend.Interfaces;
using backend.Mapper;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Service
{
    public class MeetupService : IMeetupService
    {
        private readonly ApplicationDBContext _context;
        private readonly IMeetupRepository _meetupRepository;
        private readonly IUserRepository _userRepository;

        public MeetupService(
            ApplicationDBContext context,
            IMeetupRepository meetupRepository,
            IUserRepository userRepository)
        {
            _context = context;
            _meetupRepository = meetupRepository;
            _userRepository = userRepository;
        }

        public async Task<MeetupEventDto> CreateMeetupAsync(ClaimsPrincipal principal, CreateMeetupDto dto)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var activities = BuildActivities(dto.Activities);
            var meetup = MeetupMapper.ToMeetupModel(dto, user, activities);
            var created = await _meetupRepository.CreateAsync(meetup);
            return MeetupMapper.ToMeetupEventDto(created);
        }

        public async Task<MeetupEventDto> GetMeetupAsync(int meetupId)
        {
            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            return MeetupMapper.ToMeetupEventDto(meetup);
        }

        public async Task<List<MeetupEventDto>> GetAllMeetupsAsync(int? skip = 0, int? take = 20)
        {
            var normalizedSkip = Math.Max(0, skip ?? 0);
            var normalizedTake = take.HasValue && take.Value > 0 ? take.Value : 20;

            var meetups = await _meetupRepository.GetAllActiveAsync();
            return meetups
                .Skip(normalizedSkip)
                .Take(normalizedTake)
                .Select(meetup => MeetupMapper.ToMeetupEventDto(meetup))
                .ToList();
        }

        public async Task<List<MeetupEventDto>> GetMeetupsByLocationAsync(string region, string suburb)
        {
            if (string.IsNullOrWhiteSpace(region) || string.IsNullOrWhiteSpace(suburb))
            {
                throw new InvalidOperationException("Region and suburb are required.");
            }

            var meetups = await _meetupRepository.GetByLocationAsync(region.Trim(), suburb.Trim());
            return meetups
                .Select(meetup => MeetupMapper.ToMeetupEventDto(meetup))
                .ToList();
        }

        public async Task<List<MeetupEventDto>> GetMeetupsByActivityAsync(int activityId)
        {
            var meetups = await _meetupRepository.GetByActivityAsync(activityId);
            return meetups
                .Select(meetup => MeetupMapper.ToMeetupEventDto(meetup))
                .ToList();
        }

        public async Task<List<MeetupEventDto>> GetUpcomingMeetupsAsync(int daysAhead = 30)
        {
            if (daysAhead <= 0)
            {
                throw new InvalidOperationException("Days ahead must be greater than zero.");
            }

            var fromDate = DateTime.UtcNow.Date;
            var toDate = fromDate.AddDays(daysAhead);
            var meetups = await _meetupRepository.GetUpcomingAsync(fromDate, toDate);

            return meetups
                .Select(meetup => MeetupMapper.ToMeetupEventDto(meetup))
                .ToList();
        }

        public async Task<MeetupEventDto> UpdateMeetupAsync(ClaimsPrincipal principal, int meetupId, UpdateMeetupDto dto)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            EnsureCreator(user, meetup);

            if (dto.Activities != null)
            {
                _context.Activities.RemoveRange(meetup.Activities);

                var activities = BuildActivities(dto.Activities);
                meetup.Activities = activities;
            }

            MeetupMapper.ApplyMeetupUpdate(meetup, dto);

            var updated = await _meetupRepository.UpdateAsync(meetup);
            return MeetupMapper.ToMeetupEventDto(updated);
        }

        public async Task DeleteMeetupAsync(ClaimsPrincipal principal, int meetupId)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            EnsureCreator(user, meetup);

            await _meetupRepository.DeleteAsync(meetupId);
        }

        public async Task<UserMeetupDto> JoinMeetupAsync(ClaimsPrincipal principal, int meetupId)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            if (meetup.CreatorId == user.Id)
            {
                throw new InvalidOperationException("Creators cannot join their own meetups.");
            }

            if (meetup.Status == MeetupStatus.Cancelled || meetup.Status == MeetupStatus.Completed)
            {
                throw new InvalidOperationException("Meetup is not open for joining.");
            }

            if (await _meetupRepository.HasUserJoinedAsync(user.Id, meetupId))
            {
                throw new InvalidOperationException("You have already joined this meetup.");
            }

            var activeParticipants = meetup.Participants.Count(participant =>
                participant.Status != UserMeetup.ParticipantStatus.Left &&
                participant.Status != UserMeetup.ParticipantStatus.Rejected);

            if (activeParticipants >= meetup.MaxParticipants)
            {
                throw new InvalidOperationException("Meetup is full.");
            }

            var userMeetup = new UserMeetup
            {
                UserId = user.Id,
                MeetupEventId = meetup.Id,
                Status = UserMeetup.ParticipantStatus.Pending,
                JoinedAt = DateTime.UtcNow
            };

            _context.UserMeetups.Add(userMeetup);
            await _context.SaveChangesAsync();

            userMeetup.User = user;
            return MeetupMapper.ToUserMeetupDto(userMeetup);
        }

        public async Task LeaveMeetupAsync(ClaimsPrincipal principal, int meetupId)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var userMeetup = await _context.UserMeetups
                .FirstOrDefaultAsync(item => item.MeetupEventId == meetupId && item.UserId == user.Id);

            if (userMeetup == null)
            {
                throw new InvalidOperationException("You have not joined this meetup.");
            }

            userMeetup.Status = UserMeetup.ParticipantStatus.Left;
            _context.UserMeetups.Update(userMeetup);
            await _context.SaveChangesAsync();
        }

        public async Task<List<MeetupEventDto>> GetUserJoinedMeetupsAsync(ClaimsPrincipal principal)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetups = await _meetupRepository.GetUserJoinedMeetupsAsync(user.Id);
            return meetups
                .Select(meetup => MeetupMapper.ToMeetupEventDto(meetup))
                .ToList();
        }

        public async Task<List<MeetupEventDto>> GetUserCreatedMeetupsAsync(ClaimsPrincipal principal)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetups = await _meetupRepository.GetByCreatorAsync(user.Id);
            return meetups
                .Select(meetup => MeetupMapper.ToMeetupEventDto(meetup))
                .ToList();
        }

        public async Task<List<UserMeetupDto>> GetMeetupParticipantsAsync(int meetupId)
        {
            var participants = await _meetupRepository.GetMeetupParticipantsAsync(meetupId);
            return participants
                .Select(participant => MeetupMapper.ToUserMeetupDto(participant))
                .ToList();
        }

        public async Task<bool> IsUserJoinedAsync(ClaimsPrincipal principal, int meetupId)
        {
            var user = await GetCurrentUserAsync(principal);
            return await _meetupRepository.HasUserJoinedAsync(user.Id, meetupId);
        }

        public async Task<UserMeetupDto> ConfirmParticipantAsync(ClaimsPrincipal principal, int meetupId, int participantId)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            EnsureCreator(user, meetup);

            var updatedParticipant = await _meetupRepository.ConfirmParticipantAsync(meetupId, participantId);
            return MeetupMapper.ToUserMeetupDto(updatedParticipant);
        }

        public async Task RejectParticipantAsync(ClaimsPrincipal principal, int meetupId, int participantId)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            EnsureCreator(user, meetup);

            await _meetupRepository.RemoveParticipantAsync(meetupId, participantId);
        }

        private static List<Activity> BuildActivities(IReadOnlyCollection<ActivityInputDto> activityInputs)
        {
            if (activityInputs.Count == 0)
            {
                throw new InvalidOperationException("At least one activity is required.");
            }

            if (activityInputs.Count > 3)
            {
                throw new InvalidOperationException("A meetup can have up to 3 activities.");
            }

            var seenOrders = new HashSet<int>();
            var activities = new List<Activity>(activityInputs.Count);

            foreach (var input in activityInputs)
            {
                if (string.IsNullOrWhiteSpace(input.Name))
                {
                    throw new InvalidOperationException("Activity name is required.");
                }

                if (input.Order is < 1 or > 3)
                {
                    throw new InvalidOperationException("Activity order must be between 1 and 3.");
                }

                if (!seenOrders.Add(input.Order))
                {
                    throw new InvalidOperationException("Activity order must be unique.");
                }

                if (!Enum.TryParse<ActivityType>(input.Type?.Trim(), true, out var parsedType))
                {
                    throw new InvalidOperationException("Invalid activity type.");
                }

                activities.Add(new Activity
                {
                    Name = input.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(input.Description)
                        ? null
                        : input.Description.Trim(),
                    Type = parsedType,
                    Order = input.Order,
                    MeetupEvent = null!
                });
            }

            return activities;
        }

        public async Task<List<UserMeetupDto>> GetPendingParticipantsAsync(int meetupId)
        {
            var participants = await _meetupRepository.GetMeetupParticipantsAsync(meetupId, "Pending");
            return participants
                .Select(participant => MeetupMapper.ToUserMeetupDto(participant))
                .ToList();
        }

        public async Task<MeetupEventDto> UpdateMeetupStatusAsync(ClaimsPrincipal principal, int meetupId, string status)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            EnsureCreator(user, meetup);

            if (!Enum.TryParse<MeetupStatus>(status.Trim(), true, out var parsedStatus))
            {
                throw new InvalidOperationException("Invalid status.");
            }

            meetup.Status = parsedStatus;
            meetup.UpdatedAt = DateTime.UtcNow;

            if (parsedStatus == MeetupStatus.Confirmed)
            {
                meetup.ConfirmedAt = DateTime.UtcNow;
            }

            if (parsedStatus == MeetupStatus.Completed)
            {
                meetup.CompletedAt = DateTime.UtcNow;
            }

            var updated = await _meetupRepository.UpdateAsync(meetup);
            return MeetupMapper.ToMeetupEventDto(updated);
        }

        public async Task<MeetupLocationSuggestionDto> SuggestLocationAsync(ClaimsPrincipal principal, int meetupId, SuggestLocationDto dto)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);

            var suggestion = MeetupMapper.ToLocationSuggestion(meetup.Id, user.Id, dto);
            _context.MeetupLocationSuggestions.Add(suggestion);
            await _context.SaveChangesAsync();

            suggestion.SuggestedByUser = user;
            return MeetupMapper.ToLocationSuggestionDto(suggestion);
        }

        public async Task<List<MeetupLocationSuggestionDto>> GetLocationSuggestionsAsync(int meetupId)
        {
            var suggestions = await _context.MeetupLocationSuggestions
                .Include(suggestion => suggestion.SuggestedByUser)
                .Where(suggestion => suggestion.MeetupEventId == meetupId)
                .OrderBy(suggestion => suggestion.CreatedAt)
                .ToListAsync();

            return suggestions
                .Select(suggestion => MeetupMapper.ToLocationSuggestionDto(suggestion))
                .ToList();
        }

        public async Task<MeetupLocationSuggestionDto> ChooseLocationAsync(
            ClaimsPrincipal principal,
            int meetupId,
            int locationSuggestionId)
        {
            var user = await GetCurrentUserAsync(principal);
            EnsureVerified(user);

            var meetup = await _meetupRepository.GetByIdAsync(meetupId);
            EnsureCreator(user, meetup);

            var suggestions = await _context.MeetupLocationSuggestions
                .Where(suggestion => suggestion.MeetupEventId == meetupId)
                .ToListAsync();

            var selected = suggestions
                .FirstOrDefault(suggestion => suggestion.Id == locationSuggestionId);

            if (selected == null)
            {
                throw new InvalidOperationException("Location suggestion not found.");
            }

            foreach (var suggestion in suggestions)
            {
                suggestion.IsChosen = false;
            }

            selected.IsChosen = true;
            meetup.LocationName = selected.Name;
            meetup.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            selected.SuggestedByUser = await _userRepository.GetUserByIdAsync(selected.SuggestedByUserId)
                ?? selected.SuggestedByUser;

            return MeetupMapper.ToLocationSuggestionDto(selected);
        }

        private async Task<AppUser> GetCurrentUserAsync(ClaimsPrincipal principal)
        {
            var userIdValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? principal.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? throw new UnauthorizedAccessException("User identity is missing.");

            if (!int.TryParse(userIdValue, out var userId))
            {
                throw new UnauthorizedAccessException("User identity is invalid.");
            }

            return await _userRepository.GetUserByIdAsync(userId)
                ?? throw new UnauthorizedAccessException("User not found.");
        }

        private static void EnsureVerified(AppUser user)
        {
            if (!user.IsVerified)
            {
                throw new InvalidOperationException("Face verification is required before hosting meetups.");
            }
        }

        private static void EnsureCreator(AppUser user, MeetupEvent meetup)
        {
            if (meetup.CreatorId != user.Id)
            {
                throw new UnauthorizedAccessException("Only the creator can modify this meetup.");
            }
        }
    }
}
