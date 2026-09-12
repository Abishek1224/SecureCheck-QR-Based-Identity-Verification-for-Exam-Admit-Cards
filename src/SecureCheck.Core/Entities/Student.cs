namespace SecureCheck.Core.Entities;

public class Student
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string RollNumber { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty; // path/URL to the verified student photo
    public string Subject { get; set; } = string.Empty;
    public string ExamCentre { get; set; } = string.Empty;
    public DateTime RegisteredAtUtc { get; set; } = DateTime.UtcNow;

    public ExamRegistration? Registration { get; set; }
}
