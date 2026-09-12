using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SecureCheck.Core.DTOs;
using SecureCheck.Core.Entities;
using SecureCheck.Core.Enums;
using SecureCheck.Infrastructure.Data;
using Xunit;

namespace SecureCheck.Tests;

public class ApiFlowTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;
    private readonly TestWebApplicationFactory _factory;

    public ApiFlowTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Registration_WithDuplicateRollNumber_ReturnsConflict()
    {
        var request = BuildRegistrationRequest($"ROLL-{Guid.NewGuid():N}");

        var first = await _client.PostAsJsonAsync("/api/registration", request);
        var second = await _client.PostAsJsonAsync("/api/registration", request);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Registration_ReturnsAdmitCardUrl_And_VerificationReturnsStringEnum()
    {
        var request = BuildRegistrationRequest($"ROLL-{Guid.NewGuid():N}");
        var registerResponse = await _client.PostAsJsonAsync("/api/registration", request);
        registerResponse.EnsureSuccessStatusCode();

        var registrationPayload = await registerResponse.Content.ReadFromJsonAsync<RegistrationResponseDto>();
        Assert.NotNull(registrationPayload);
        Assert.False(string.IsNullOrWhiteSpace(registrationPayload!.AdmitCardUrl));

        var token = await GetQrTokenAsync(registrationPayload.RegistrationId);

        using var verifyRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/verify/{registrationPayload.RegistrationId}?token={Uri.EscapeDataString(token)}");
        verifyRequest.Headers.Add("X-Invigilator-Api-Key", "test-invigilator-api-key");

        var verifyResponse = await _client.SendAsync(verifyRequest);
        verifyResponse.EnsureSuccessStatusCode();

        using var responseJson = JsonDocument.Parse(await verifyResponse.Content.ReadAsStringAsync());
        Assert.Equal("Verified", responseJson.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Verification_WithoutApiKey_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync($"/api/verify/{Guid.NewGuid()}?token=test");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Verification_RevokedRegistration_ReturnsCardRevoked_AndLogsAttempt()
    {
        var registration = await RegisterCandidateAsync($"ROLL-{Guid.NewGuid():N}");

        await using (var scope = _factory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<SecureCheckDbContext>();
            var entity = await db.Registrations.FindAsync(registration.RegistrationId);
            Assert.NotNull(entity);
            entity!.IsActive = false;
            await db.SaveChangesAsync();
        }

        using var verifyRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/verify/{registration.RegistrationId}?token={Uri.EscapeDataString(registration.Token)}");
        verifyRequest.Headers.Add("X-Invigilator-Api-Key", "test-invigilator-api-key");

        var verifyResponse = await _client.SendAsync(verifyRequest);
        verifyResponse.EnsureSuccessStatusCode();

        var payload = await verifyResponse.Content.ReadFromJsonAsync<VerificationResponseDto>();
        Assert.NotNull(payload);
        Assert.Equal(VerificationStatus.CardRevoked, payload!.Status);

        await using var assertionScope = _factory.Services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<SecureCheckDbContext>();
        var hasLog = assertionDb.VerificationLogs.Any(
            x => x.RegistrationId == registration.RegistrationId && x.Status == VerificationStatus.CardRevoked);
        Assert.True(hasLog);
    }

    [Fact]
    public async Task Verification_ExpectedExamMismatch_ReturnsExamMismatch_AndLogsAttempt()
    {
        var registration = await RegisterCandidateAsync($"ROLL-{Guid.NewGuid():N}");

        using var verifyRequest = new HttpRequestMessage(
            HttpMethod.Get,
            $"/api/verify/{registration.RegistrationId}?token={Uri.EscapeDataString(registration.Token)}&expectedExamName=DifferentExam");
        verifyRequest.Headers.Add("X-Invigilator-Api-Key", "test-invigilator-api-key");

        var verifyResponse = await _client.SendAsync(verifyRequest);
        verifyResponse.EnsureSuccessStatusCode();

        var payload = await verifyResponse.Content.ReadFromJsonAsync<VerificationResponseDto>();
        Assert.NotNull(payload);
        Assert.Equal(VerificationStatus.ExamMismatch, payload!.Status);

        await using var assertionScope = _factory.Services.CreateAsyncScope();
        var assertionDb = assertionScope.ServiceProvider.GetRequiredService<SecureCheckDbContext>();
        var hasLog = assertionDb.VerificationLogs.Any(
            x => x.RegistrationId == registration.RegistrationId && x.Status == VerificationStatus.ExamMismatch);
        Assert.True(hasLog);
    }

    private async Task<(Guid RegistrationId, string Token)> RegisterCandidateAsync(string rollNumber)
    {
        var request = BuildRegistrationRequest(rollNumber);
        var response = await _client.PostAsJsonAsync("/api/registration", request);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<RegistrationResponseDto>();
        Assert.NotNull(payload);

        var token = await GetQrTokenAsync(payload!.RegistrationId);
        return (payload.RegistrationId, token);
    }

    private async Task<string> GetQrTokenAsync(Guid registrationId)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SecureCheckDbContext>();
        var registration = await db.Registrations.FindAsync(registrationId);
        Assert.NotNull(registration);
        return registration!.QrCodeToken;
    }

    private static RegistrationRequestDto BuildRegistrationRequest(string rollNumber) =>
        new()
        {
            FullName = "Test Student",
            RollNumber = rollNumber,
            PhotoUrl = "https://example.com/photo.png",
            Subject = "Computer Science",
            ExamCentre = "Main Hall",
            ExamName = "Midterm",
            ExamDateUtc = DateTime.UtcNow.AddDays(2),
            SeatNumber = "A-12"
        };
}
