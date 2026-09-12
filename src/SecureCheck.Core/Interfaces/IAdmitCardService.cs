using SecureCheck.Core.Entities;

namespace SecureCheck.Core.Interfaces;

public interface IAdmitCardService
{
    // Builds the printable admit-card PDF and returns the saved file path.
    Task<string> GenerateAdmitCardAsync(Student student, ExamRegistration registration, byte[] qrCodePng);
}
