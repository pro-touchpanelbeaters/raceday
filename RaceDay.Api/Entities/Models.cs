namespace RaceDay.Api.Entities;

public enum UserRole { Organiser, Participant }
public enum EventType { Run, Walk, Cycle }
public enum EnrolmentStatus { Pending, Confirmed, Cancelled }

public class User
{
    public int UserId { get; set; }
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public UserRole Role { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public class Event
{
    public int EventId { get; set; }
    public int OrganiserId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = null!;
    public decimal Distance { get; set; }
    public EventType EventType { get; set; }
    public string? BannerImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User Organiser { get; set; } = null!;
    public ICollection<Category> Categories { get; set; } = new List<Category>();
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
    public ICollection<EventImage> Images { get; set; } = new List<EventImage>();
}

public class Category
{
    public int CategoryId { get; set; }
    public int EventId { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int? MinAge { get; set; }
    public int? MaxAge { get; set; }
    public decimal? Distance { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Event Event { get; set; } = null!;
    public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>();
}

public class Enrolment
{
    public int EnrolmentId { get; set; }
    public int ParticipantId { get; set; }
    public int EventId { get; set; }
    public int CategoryId { get; set; }
    public DateTime EnrolmentDate { get; set; }
    public EnrolmentStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public User Participant { get; set; } = null!;
    public Event Event { get; set; } = null!;
    public Category Category { get; set; } = null!;
    public Result? Result { get; set; }
}

public class Result
{
    public int ResultId { get; set; }
    public int EnrolmentId { get; set; }
    public TimeSpan? FinishTime { get; set; }
    public int? FinishingPosition { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public Enrolment Enrolment { get; set; } = null!;
}

public class EventImage
{
    public int EventImageId { get; set; }
    public int EventId { get; set; }
    public string BlobName { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public bool IsBanner { get; set; }
    public DateTime CreatedAt { get; set; }
    public Event Event { get; set; } = null!;
}
