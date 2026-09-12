using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SecureCheck.Core.DTOs;
using SecureCheck.Core.Entities;
using SecureCheck.Core.Interfaces;
using SecureCheck.Infrastructure.Data;

namespace SecureCheck.Api.Controllers;

[ApiController]
[Route("api/registration")]
public class RegistrationController : ControllerBase
{
    private readonly SecureCheckDbContext _context;
    private readonly IQrCodeService _qrCodeService;

    public RegistrationController(
        SecureCheckDbContext context,
        IQrCodeService qrCodeService)
    {
        _context = context;
        _qrCodeService = qrCodeService;
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
        var existingStudent = await _context.Students
            .FirstOrDefaultAsync(s => s.RollNumber == request.RollNumber);

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

        // Save student and registration
        _context.Students.Add(student);
        _context.Registrations.Add(registration);

        await _context.SaveChangesAsync();

        // Create verification URL
        var verificationUrl =
            $"{Request.Scheme}://{Request.Host}/api/verify/{registration.Id}" +
            $"?token={Uri.EscapeDataString(qrToken)}";

        // Generate QR code
        var qrBytes = _qrCodeService.GenerateQrCode(verificationUrl);

        // Create QR code folder
        var qrFolder = Path.Combine(
            Directory.GetCurrentDirectory(),
            "wwwroot",
            "qrcodes");

        Directory.CreateDirectory(qrFolder);

        // Save QR code image
        var qrFileName = $"{registration.Id}.png";
        var qrFilePath = Path.Combine(qrFolder, qrFileName);

        await System.IO.File.WriteAllBytesAsync(qrFilePath, qrBytes);

        // URL that can be used to display the QR image
        var qrCodeUrl =
            $"{Request.Scheme}://{Request.Host}/qrcodes/{qrFileName}";

        return Ok(new RegistrationResponseDto
        {
            RegistrationId = registration.Id,
            FullName = student.FullName,
            RollNumber = student.RollNumber,
            ExamName = registration.ExamName,
            ExamDateUtc = registration.ExamDateUtc,
            SeatNumber = registration.SeatNumber,
            QrCodeUrl = qrCodeUrl,
            Message = "Student registered successfully."
        });
    }
}