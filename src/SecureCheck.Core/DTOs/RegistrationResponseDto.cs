namespace SecureCheck.Core.DTOs;

public class RegistrationResponseDto
{
    public Guid RegistrationId { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string RollNumber { get; set; } = string.Empty;

    public string ExamName { get; set; } = string.Empty;

    public DateTime ExamDateUtc { get; set; }

    public string SeatNumber { get; set; } = string.Empty;

    public string QrCodeUrl { get; set; } = string.Empty;

    public string? AdmitCardUrl { get; set; }

    public string Message { get; set; } = string.Empty;
}