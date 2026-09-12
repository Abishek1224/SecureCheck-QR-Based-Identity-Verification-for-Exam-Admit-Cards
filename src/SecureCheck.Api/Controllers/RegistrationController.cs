using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using SecureCheck.Core.DTOs;
using SecureCheck.Core.Entities;
using SecureCheck.Core.Interfaces;

namespace SecureCheck.Api.Controllers;

[ApiController]
[Route("api/registration")]
public class RegistrationController : ControllerBase
{
    private readonly IRegistrationRepository _repository;
    private readonly IQrCodeService _qrCodeService;
    private readonly IAdmitCardService _admitCardService;
    private readonly IWebHostEnvironment _environment;

    public RegistrationController(
        IRegistrationRepository repository,
        IQrCodeService qrCodeService,
        IAdmitCardService admitCardService,
        IWebHostEnvironment environment)
    {
        _repository = repository;
        _qrCodeService = qrCodeService;
        _admitCardService = admitCardService;
        _environment = environment;
    }

    [HttpPost]
    public async Task<ActionResult<RegistrationResponseDto>> Register(
        RegistrationRequestDto request)
    {
        // Validate required fields
        if (string.IsNullOrWhiteSpace(request.FullName) ||
            string.IsNullOrWhiteSpace(request.RollNumber) ||
            string.IsNullOrWhiteSpace(request.Subject) ||
            string.IsNullOrWhiteSpace(request.ExamName) ||
            string.IsNullOrWhiteSpace(request.SeatNumber))
        {
            return BadRequest(new
            {
                message = "Required student and exam details are missing."
            });
        }

        // Check whether roll number is already registered
        var existingStudent = await _repository.GetByRollNumberAsync(request.RollNumber);

        if (existingStudent != null)
        {
            return Conflict(new
            {
                message = "A student with this roll number is already registered."
            });
        }

        // Create student
        var student = new Student
        {
            FullName = request.FullName,
            RollNumber = request.RollNumber,
            PhotoUrl = request.PhotoUrl,
            Subject = request.Subject,
            ExamCentre = request.ExamCentre
        };

        // Generate secure QR token
        var qrToken = Convert.ToBase64String(
            RandomNumberGenerator.GetBytes(32));

        // Create exam registration
        var registration = new ExamRegistration
        {
            StudentId = student.Id,
            Student = student,
            ExamName = request.ExamName,
            ExamDateUtc = request.ExamDateUtc,
            SeatNumber = request.SeatNumber,
            QrCodeToken = qrToken,
            IsActive = true
        };

        await _repository.AddAsync(student, registration);

        // Create verification URL
        var verificationUrl =
            $"{Request.Scheme}://{Request.Host}/api/verify/{registration.Id}" +
            $"?token={Uri.EscapeDataString(qrToken)}";

        // Generate QR code
        var qrBytes = _qrCodeService.GenerateQrCode(verificationUrl);

        // Create QR code folder
        var qrFolder = Path.Combine(_environment.WebRootPath, "qrcodes");

        Directory.CreateDirectory(qrFolder);

        // Save QR code image
        var qrFileName = $"{registration.Id}.png";
        var qrFilePath = Path.Combine(qrFolder, qrFileName);

        await System.IO.File.WriteAllBytesAsync(qrFilePath, qrBytes);

        await _admitCardService.GenerateAdmitCardAsync(student, registration, qrBytes);
        await _repository.SaveChangesAsync();

        // URL that can be used to display the QR image
        var qrCodeUrl =
            $"{Request.Scheme}://{Request.Host}/qrcodes/{qrFileName}";
        var admitCardUrl =
            $"{Request.Scheme}://{Request.Host}/admitcards/{registration.Id}.pdf";

        return Ok(new RegistrationResponseDto
        {
            RegistrationId = registration.Id,
            FullName = student.FullName,
            RollNumber = student.RollNumber,
            ExamName = registration.ExamName,
            ExamDateUtc = registration.ExamDateUtc,
            SeatNumber = registration.SeatNumber,
            QrCodeUrl = qrCodeUrl,
            AdmitCardUrl = admitCardUrl,
            Message = "Student registered successfully."
        });
    }
}