using SecureCheck.Core.Enums;

namespace SecureCheck.Core.Entities;

// Every scan attempt, successful or not - feeds the future "analytics dashboard".
public class VerificationLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RegistrationId { get; set; }
    public string InvigilatorDeviceId { get; set; } = string.Empty;
    public VerificationStatus Status { get; set; }
    public DateTime ScannedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
