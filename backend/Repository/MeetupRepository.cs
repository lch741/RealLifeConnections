using backend.Data;
using backend.Interfaces;
using backend.Models;
using Microsoft.EntityFrameworkCore;

namespace backend.Repository
{
    public class MeetupRepository : IMeetupRepository
    {
        private readonly ApplicationDBContext _context;

        public MeetupRepository(ApplicationDBContext context)
        {
            _context = context;
        }

        public async Task<MeetupEvent> GetByIdAsync(int id)
        {
            var meetup = await BaseQuery()
                .FirstOrDefaultAsync(meetup => meetup.Id == id);

            return meetup ?? throw new InvalidOperationException("Meetup not found.");
        }

        public Task<List<MeetupEvent>> GetAllActiveAsync()
        {
            return BaseQuery()
                .Where(meetup => meetup.Status != MeetupStatus.Cancelled && meetup.Status != MeetupStatus.Completed)
                .OrderBy(meetup => meetup.EventDate)
                .ThenBy(meetup => meetup.StartTime)
                .ToListAsync();
        }

        public Task<List<MeetupEvent>> GetByLocationAsync(string region, string suburb)
        {
            return BaseQuery()
                .Where(meetup =>
                    EF.Functions.ILike(meetup.Region, region) &&
                    EF.Functions.ILike(meetup.Suburb, suburb))
                .OrderBy(meetup => meetup.EventDate)
                .ThenBy(meetup => meetup.StartTime)
                .ToListAsync();
        }

        public Task<List<MeetupEvent>> GetByActivityAsync(int activityId)
        {
            return BaseQuery()
                .Where(meetup => meetup.ActivityId == activityId)
                .OrderBy(meetup => meetup.EventDate)
                .ThenBy(meetup => meetup.StartTime)
                .ToListAsync();
        }

        public Task<List<MeetupEvent>> GetByCreatorAsync(int creatorId)
        {
            return BaseQuery()
                .Where(meetup => meetup.CreatorId == creatorId)
                .OrderByDescending(meetup => meetup.CreatedAt)
                .ToListAsync();
        }

        public Task<List<MeetupEvent>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return BaseQuery()
                .Where(meetup => meetup.EventDate >= startDate && meetup.EventDate <= endDate)
                .OrderBy(meetup => meetup.EventDate)
                .ThenBy(meetup => meetup.StartTime)
                .ToListAsync();
        }

        public async Task<MeetupEvent> CreateAsync(MeetupEvent meetup)
        {
            _context.MeetupEvents.Add(meetup);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(meetup.Id);
        }

        public async Task<MeetupEvent> UpdateAsync(MeetupEvent meetup)
        {
            _context.MeetupEvents.Update(meetup);
            await _context.SaveChangesAsync();
            return await GetByIdAsync(meetup.Id);
        }

        public async Task DeleteAsync(int id)
        {
            var meetup = await _context.MeetupEvents
                .FirstOrDefaultAsync(meetup => meetup.Id == id);

            if (meetup == null)
            {
                throw new InvalidOperationException("Meetup not found.");
            }

            _context.MeetupEvents.Remove(meetup);
            await _context.SaveChangesAsync();
        }

        public Task<List<MeetupEvent>> GetByStatusAsync(MeetupStatus status)
        {
            return BaseQuery()
                .Where(meetup => meetup.Status == status)
                .OrderBy(meetup => meetup.EventDate)
                .ThenBy(meetup => meetup.StartTime)
                .ToListAsync();
        }

        public Task<List<MeetupEvent>> GetUpcomingAsync(DateTime fromDate, DateTime toDate)
        {
            return BaseQuery()
                .Where(meetup => meetup.EventDate >= fromDate && meetup.EventDate <= toDate)
                .OrderBy(meetup => meetup.EventDate)
                .ThenBy(meetup => meetup.StartTime)
                .ToListAsync();
        }

        public Task<bool> HasUserJoinedAsync(int userId, int meetupId)
        {
            return _context.UserMeetups
                .AnyAsync(userMeetup =>
                    userMeetup.UserId == userId &&
                    userMeetup.MeetupEventId == meetupId &&
                    userMeetup.Status != UserMeetup.ParticipantStatus.Left &&
                    userMeetup.Status != UserMeetup.ParticipantStatus.Rejected);
        }

        public Task<List<MeetupEvent>> GetUserJoinedMeetupsAsync(int userId)
        {
            return BaseQuery()
                .Where(meetup => meetup.Participants.Any(participant =>
                    participant.UserId == userId &&
                    participant.Status != UserMeetup.ParticipantStatus.Left &&
                    participant.Status != UserMeetup.ParticipantStatus.Rejected))
                .OrderByDescending(meetup => meetup.EventDate)
                .ToListAsync();
        }

        public Task<List<UserMeetup>> GetMeetupParticipantsAsync(int meetupId, string? status = null)
        {
            var query = _context.UserMeetups
                .Include(userMeetup => userMeetup.User)
                .Where(userMeetup => userMeetup.MeetupEventId == meetupId)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status) &&
                Enum.TryParse<UserMeetup.ParticipantStatus>(status.Trim(), true, out var statusFilter))
            {
                query = query.Where(userMeetup => userMeetup.Status == statusFilter);
            }

            return query
                .OrderBy(userMeetup => userMeetup.JoinedAt)
                .ToListAsync();
        }

        public async Task<UserMeetup> ConfirmParticipantAsync(int meetupId, int userId)
        {
            var userMeetup = await _context.UserMeetups
                .Include(item => item.User)
                .FirstOrDefaultAsync(item => item.MeetupEventId == meetupId && item.UserId == userId);

            if (userMeetup == null)
            {
                throw new InvalidOperationException("Participant not found.");
            }

            userMeetup.Status = UserMeetup.ParticipantStatus.Approved;
            userMeetup.IsConfirmed = true;
            userMeetup.ConfirmedAt = DateTime.UtcNow;

            _context.UserMeetups.Update(userMeetup);
            await _context.SaveChangesAsync();
            return userMeetup;
        }

        public async Task RemoveParticipantAsync(int meetupId, int userId)
        {
            var userMeetup = await _context.UserMeetups
                .FirstOrDefaultAsync(item => item.MeetupEventId == meetupId && item.UserId == userId);

            if (userMeetup == null)
            {
                throw new InvalidOperationException("Participant not found.");
            }

            userMeetup.Status = UserMeetup.ParticipantStatus.Rejected;
            _context.UserMeetups.Update(userMeetup);
            await _context.SaveChangesAsync();
        }

        private IQueryable<MeetupEvent> BaseQuery()
        {
            return _context.MeetupEvents
                .Include(meetup => meetup.Creator)
                .Include(meetup => meetup.Activity)
                .Include(meetup => meetup.Participants)
                .ThenInclude(participant => participant.User)
                .Include(meetup => meetup.LocationSuggestions)
                .ThenInclude(suggestion => suggestion.SuggestedByUser)
                .Include(meetup => meetup.Feedbacks);
        }
    }
}
