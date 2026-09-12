using Microsoft.EntityFrameworkCore;
using SecureCheck.Core.Entities;
using SecureCheck.Core.Interfaces;
using SecureCheck.Infrastructure.Data;

namespace SecureCheck.Infrastructure.Repositories;

public class RegistrationRepository : IRegistrationRepository
{
    private readonly SecureCheckDbContext _db;

    public RegistrationRepository(SecureCheckDbContext db) => _db = db;

    public async Task<ExamRegistration> AddAsync(Student student, ExamRegistration registration)
    {
        student.Registration = registration;
        registration.Student = student;
        registration.StudentId = student.Id;

        _db.Students.Add(student);
        _db.Registrations.Add(registration);
        await _db.SaveChangesAsync();

        return registration;
    }

    public async Task<ExamRegistration?> GetByIdAsync(Guid registrationId)
    {
        return await _db.Registrations
            .Include(r => r.Student)
            .FirstOrDefaultAsync(r => r.Id == registrationId);
    }

    public Task<Student?> GetByRollNumberAsync(string rollNumber)
    {
        return _db.Students.FirstOrDefaultAsync(s => s.RollNumber == rollNumber);
    }

    public async Task LogVerificationAsync(VerificationLog log)
    {
        _db.VerificationLogs.Add(log);
        await _db.SaveChangesAsync();
    }

    public Task SaveChangesAsync() => _db.SaveChangesAsync();
}
