using Microsoft.AspNetCore.Http;
using RaceDay.Api.DTOs;
using RaceDay.Api.Entities;

namespace RaceDay.Api.Services;

public interface IAuthService
{
    Task<UserResponse> RegisterAsync(RegisterRequest request);
    Task<UserResponse?> LoginAsync(LoginRequest request);
}
public interface IUserService
{
    Task<UserResponse?> GetAsync(int userId);
    Task<UserResponse?> UpdateAsync(int userId, UpdateProfileRequest request);
}
public interface IEventService
{
    Task<IReadOnlyList<EventResponse>> GetAllAsync(string? search, EventType? type, DateTime? from, DateTime? to);
    Task<EventResponse?> GetAsync(int id);
    Task<EventResponse> CreateAsync(int organiserId, CreateEventRequest request);
    Task<EventResponse?> UpdateAsync(int organiserId, int id, UpdateEventRequest request);
    Task<bool> DeleteAsync(int organiserId, int id);
}
public interface ICategoryService
{
    Task<IReadOnlyList<CategoryResponse>> GetByEventAsync(int eventId);
    Task<CategoryResponse?> GetAsync(int id);
    Task<CategoryResponse> CreateAsync(int organiserId, int eventId, CreateCategoryRequest request);
    Task<CategoryResponse?> UpdateAsync(int organiserId, int id, UpdateCategoryRequest request);
    Task<bool> DeleteAsync(int organiserId, int id);
}
public interface IEnrolmentService
{
    Task<EnrolmentResponse?> CreateAsync(int participantId, int eventId, CreateEnrolmentRequest request);
    Task<IReadOnlyList<EnrolmentResponse>> GetMineAsync(int participantId);
    Task<IReadOnlyList<EnrolmentResponse>?> GetForEventAsync(int organiserId, int eventId);
    Task<bool> CancelAsync(int participantId, int enrolmentId);
}
public interface IResultService
{
    Task<ResultResponse?> CreateAsync(int organiserId, int enrolmentId, CreateResultRequest request);
    Task<IReadOnlyList<ResultResponse>> GetMineAsync(int participantId);
    Task<IReadOnlyList<ResultResponse>?> GetForEventAsync(int eventId);
    Task<ResultResponse?> UpdateAsync(int organiserId, int resultId, UpdateResultRequest request);
}
public interface IFileStorageService
{
    Task<(string BlobName, string Url)> UploadAsync(Stream stream, string fileName, string contentType, CancellationToken cancellationToken = default);
    Task DeleteAsync(string blobName, CancellationToken cancellationToken = default);
}
