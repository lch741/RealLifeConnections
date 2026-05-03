using System.ComponentModel.DataAnnotations;

namespace backend.Models
{
    public class UserMeetup
    {
        public int Id { get; set; }

        public int UserId { get; set; }
        public AppUser User { get; set; }

        public int MeetupEventId { get; set; }
        public MeetupEvent MeetupEvent { get; set; }

        public enum ParticipantStatus
        {
            Pending = 0,    // Awaiting creator approval
            Approved = 1,   // Approved to attend
            Rejected = 2,   // Rejected by creator
            Left = 3        // User cancelled
        }

        public ParticipantStatus Status { get; set; } = ParticipantStatus.Pending;

        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

        // Confirmation: user confirms attendance
        public bool IsConfirmed { get; set; } = false;
        public DateTime? ConfirmedAt { get; set; }
    }
}
