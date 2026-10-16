using System.ComponentModel.DataAnnotations;
using RaceDay.Api.Entities;

namespace RaceDay.Api.DTOs;

public record RegisterRequest(
    [Required, MaxLength(50)] string FirstName,
    [Required, MaxLength(50)] string LastName,
    [Required, EmailAddress] string Email,
    [Required, MinLength(8)] string Password,
    [MaxLength(20)] string? PhoneNumber,
    UserRole Role);

public record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password);

public record UserResponse(int UserId, string FirstName, string LastName, string Email, string? PhoneNumber, UserRole Role, string? ProfileImageUrl);

public record UpdateProfileRequest(
    [Required, MaxLength(50)] string FirstName,
    [Required, MaxLength(50)] string LastName,
    [MaxLength(20)] string? PhoneNumber);

public record CreateEventRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(400)] string? Description,
    DateTime EventDate,
    [Required, MaxLength(200)] string Location,
    [Range(0.1, 9999)] decimal Distance,
    EventType EventType,
    string? BannerImageUrl);

public record UpdateEventRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(400)] string? Description,
    DateTime EventDate,
    [Required, MaxLength(200)] string Location,
    [Range(0.1, 9999)] decimal Distance,
    EventType EventType,
    string? BannerImageUrl);

public record EventResponse(int EventId, int OrganiserId, string Name, string? Description, DateTime EventDate, string Location, decimal Distance, EventType EventType, string? BannerImageUrl, int EnrolmentCount);

public record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(200)] string? Description,
    [Range(0, 150)] int? MinAge,
    [Range(0, 150)] int? MaxAge,
    [Range(0.1, 9999)] decimal? Distance);

public record UpdateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(200)] string? Description,
    [Range(0, 150)] int? MinAge,
    [Range(0, 150)] int? MaxAge,
    [Range(0.1, 9999)] decimal? Distance);

public record CategoryResponse(int CategoryId, int EventId, string Name, string? Description, int? MinAge, int? MaxAge, decimal? Distance);

public record CreateEnrolmentRequest(int CategoryId);
public record EnrolmentResponse(int EnrolmentId, int ParticipantId, string ParticipantName, int EventId, string EventName, int CategoryId, string CategoryName, DateTime EnrolmentDate, EnrolmentStatus Status);

public record CreateResultRequest(TimeSpan? FinishTime, [Range(1, int.MaxValue)] int? FinishingPosition);
public record UpdateResultRequest(TimeSpan? FinishTime, [Range(1, int.MaxValue)] int? FinishingPosition, bool Publish);
public record ResultResponse(int ResultId, int EnrolmentId, string ParticipantName, string EventName, DateTime EventDate, string CategoryName, TimeSpan? FinishTime, int? FinishingPosition, DateTime? PublishedAt);

public record EventImageResponse(int EventImageId, int EventId, string ImageUrl, bool IsBanner);
