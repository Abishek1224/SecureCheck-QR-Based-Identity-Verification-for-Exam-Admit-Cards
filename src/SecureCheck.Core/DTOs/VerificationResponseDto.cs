using SecureCheck.Core.Enums;

namespace SecureCheck.Core.DTOs;

public class VerificationResponseDto
{
    public VerificationStatus Status { get; set; }
    public string? FullName { get; set; }
    public string? RollNumber { get; set; }
    public string? Subject { get; set; }
    public string? PhotoUrl { get; set; }
    public string? ExamName { get; set; }
    public string Message { get; set; } = string.Empty;
}
