using Microsoft.AspNetCore.Mvc;
using SecureCheck.Core.DTOs;
using SecureCheck.Core.Entities;
using SecureCheck.Core.Enums;
using SecureCheck.Core.Interfaces;

namespace SecureCheck.Api.Controllers;

[ApiController]
[Route("api/verify")]
public class VerificationController : ControllerBase
{
    private const string ApiKeyHeaderName = "X-Invigilator-Api-Key";
    private readonly IRegistrationRepository _repository;
    private readonly IConfiguration _configuration;

    public VerificationController(
        IRegistrationRepository repository,
        IConfiguration configuration)
    {
        _repository = repository;
        _configuration = configuration;
    }

    // GET api/verify/{registrationId}?token=...
    [HttpGet("{registrationId:guid}")]
    public async Task<ActionResult<VerificationResponseDto>> Verify(
        Guid registrationId,
        [FromQuery] string token,
        [FromQuery] string? expectedExamName = null)
    {
        var configuredApiKey = _configuration["InvigilatorAuth:ApiKey"];
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                message = "Invigilator authentication is not configured."
            });
        }

        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var providedApiKey)
            || !string.Equals(providedApiKey.ToString(), configuredApiKey, StringComparison.Ordinal))
        {
            return Unauthorized(new
            {
                message = "Invigilator authorization failed."
            });
        }

        var registration = await _repository.GetByIdAsync(registrationId);

        if (registration is null)
        {
            return Ok(new VerificationResponseDto
            {
                Status = VerificationStatus.NotRegistered,
                Message = "No matching registration found."
            });
        }

        if (!registration.IsActive)
        {
            await LogAttempt(registrationId, VerificationStatus.CardRevoked);
            return Ok(new VerificationResponseDto
            {
                Status = VerificationStatus.CardRevoked,
                Message = "Admit card has been revoked."
            });
        }

        if (registration.QrCodeToken != token)
        {
            await LogAttempt(registrationId, VerificationStatus.InvalidQr);
            return Ok(new VerificationResponseDto
            {
                Status = VerificationStatus.InvalidQr,
                Message = "QR code is invalid or has been revoked."
            });
        }

        if (!string.IsNullOrWhiteSpace(expectedExamName)
            && !string.Equals(registration.ExamName, expectedExamName, StringComparison.OrdinalIgnoreCase))
        {
            await LogAttempt(registrationId, VerificationStatus.ExamMismatch);
            return Ok(new VerificationResponseDto
            {
                Status = VerificationStatus.ExamMismatch,
                Message = "QR is valid, but the candidate is registered for a different exam."
            });
        }

        await LogAttempt(registrationId, VerificationStatus.Verified);

        var student = registration.Student!;
        return Ok(new VerificationResponseDto
        {
            Status = VerificationStatus.Verified,
            FullName = student.FullName,
            RollNumber = student.RollNumber,
            Subject = student.Subject,
            PhotoUrl = student.PhotoUrl,
            ExamName = registration.ExamName,
            Message = "Identity verified."
        });
    }

    private async Task LogAttempt(Guid registrationId, VerificationStatus status)
    {
        await _repository.LogVerificationAsync(new VerificationLog
        {
            RegistrationId = registrationId,
            Status = status,
            InvigilatorDeviceId = HttpContext.Connection.Id // placeholder until device auth is wired up
        });
    }
}
