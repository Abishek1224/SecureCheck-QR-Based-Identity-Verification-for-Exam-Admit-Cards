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
    private readonly IRegistrationRepository _repository;

    public VerificationController(IRegistrationRepository repository)
    {
        _repository = repository;
    }

    // GET api/verify/{registrationId}?token=...
    // Called by the Invigilator Client right after it scans a QR code.
    // TODO: require an authenticated invigilator device before this returns candidate data.
    [HttpGet("{registrationId:guid}")]
    public async Task<ActionResult<VerificationResponseDto>> Verify(Guid registrationId, [FromQuery] string token)
    {
        var registration = await _repository.GetByIdAsync(registrationId);

        if (registration is null)
        {
            await LogAttempt(registrationId, VerificationStatus.NotRegistered);
            return Ok(new VerificationResponseDto
            {
                Status = VerificationStatus.NotRegistered,
                Message = "No matching registration found."
            });
        }

        if (registration.QrCodeToken != token || !registration.IsActive)
        {
            await LogAttempt(registrationId, VerificationStatus.InvalidQr);
            return Ok(new VerificationResponseDto
            {
                Status = VerificationStatus.InvalidQr,
                Message = "QR code is invalid or has been revoked."
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
