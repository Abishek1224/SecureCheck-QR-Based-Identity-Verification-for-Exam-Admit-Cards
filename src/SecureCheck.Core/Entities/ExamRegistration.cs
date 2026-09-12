namespace SecureCheck.Core.Entities;

// One registration = one admit card = one QR code.
public class ExamRegistration
{
    public Guid Id { get; set; } = Guid.NewGuid(); // encoded in the QR as /verify/{Id}
    public Guid StudentId { get; set; }
    public Student? Student { get; set; }

    public string ExamName { get; set; } = string.Empty;
    public DateTime ExamDateUtc { get; set; }
    public string SeatNumber { get; set; } = string.Empty;

    public string QrCodeToken { get; set; } = string.Empty; // opaque token embedded in the QR alongside the Id
    public string? AdmitCardPdfPath { get; set; }

    public bool IsActive { get; set; } = true; // lets a card be revoked without deleting history
    public ICollection<VerificationLog> VerificationLogs { get; set; } = new List<VerificationLog>();
}
