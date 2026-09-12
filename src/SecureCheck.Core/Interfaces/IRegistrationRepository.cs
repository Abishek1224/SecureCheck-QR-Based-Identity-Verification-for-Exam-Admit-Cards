using SecureCheck.Core.Entities;

namespace SecureCheck.Core.Interfaces;

public interface IRegistrationRepository
{
    Task<ExamRegistration> AddAsync(Student student, ExamRegistration registration);
    Task<ExamRegistration?> GetByIdAsync(Guid registrationId);
    Task<Student?> GetByRollNumberAsync(string rollNumber);
    Task LogVerificationAsync(VerificationLog log);
    Task SaveChangesAsync();
}
