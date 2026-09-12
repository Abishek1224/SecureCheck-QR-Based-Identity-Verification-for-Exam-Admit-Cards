using Microsoft.EntityFrameworkCore;
using SecureCheck.Core.Entities;

namespace SecureCheck.Infrastructure.Data;

public class SecureCheckDbContext : DbContext
{
    public SecureCheckDbContext(DbContextOptions<SecureCheckDbContext> options) : base(options) { }

    public DbSet<Student> Students => Set<Student>();
    public DbSet<ExamRegistration> Registrations => Set<ExamRegistration>();
    public DbSet<VerificationLog> VerificationLogs => Set<VerificationLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Student>()
            .HasOne(s => s.Registration)
            .WithOne(r => r.Student)
            .HasForeignKey<ExamRegistration>(r => r.StudentId);

        modelBuilder.Entity<ExamRegistration>()
            .HasIndex(r => r.QrCodeToken)
            .IsUnique();
    }
}
