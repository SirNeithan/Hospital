using HospitalWeb.Models;
using Microsoft.EntityFrameworkCore;

namespace HospitalWeb.Data;

public class HospitalDbContext : DbContext
{
    public HospitalDbContext(DbContextOptions<HospitalDbContext> options) : base(options) { }

    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<Bill> Bills => Set<Bill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Patient
        modelBuilder.Entity<Patient>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            e.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            e.Property(p => p.PhoneNumber).HasMaxLength(30);
            e.Property(p => p.Email).HasMaxLength(150);
            e.Property(p => p.Address).HasMaxLength(300);
            e.Property(p => p.EmergencyContactName).HasMaxLength(100);
            e.Property(p => p.EmergencyContactPhone).HasMaxLength(30);
            e.Property(p => p.Gender).HasConversion<string>();
            e.Property(p => p.BloodType).HasConversion<string>();
        });

        // Doctor
        modelBuilder.Entity<Doctor>(e =>
        {
            e.HasKey(d => d.Id);
            e.Property(d => d.FirstName).IsRequired().HasMaxLength(100);
            e.Property(d => d.LastName).IsRequired().HasMaxLength(100);
            e.Property(d => d.Specialization).HasConversion<string>();
            e.Property(d => d.PhoneNumber).HasMaxLength(30);
            e.Property(d => d.Email).HasMaxLength(150);
            e.Property(d => d.LicenseNumber).HasMaxLength(50);
            // Store available days as comma-separated string
            e.Property(d => d.AvailableDays)
             .HasConversion(
                v => string.Join(',', v.Select(x => (int)x)),
                v => string.IsNullOrEmpty(v)
                    ? new List<DayOfWeek>()
                    : v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                       .Select(x => (DayOfWeek)int.Parse(x)).ToList()
             );
        });

        // Appointment
        modelBuilder.Entity<Appointment>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Status).HasConversion<string>();
            e.Property(a => a.Reason).HasMaxLength(500);
            e.Property(a => a.Notes).HasMaxLength(1000);
            e.Property(a => a.PatientName).HasMaxLength(200);
            e.Property(a => a.DoctorName).HasMaxLength(200);
        });

        // Room
        modelBuilder.Entity<Room>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.RoomNumber).IsRequired().HasMaxLength(20);
            e.Property(r => r.Type).HasConversion<string>();
            e.Property(r => r.Status).HasConversion<string>();
            e.Property(r => r.Floor).HasMaxLength(10);
            e.Property(r => r.Ward).HasMaxLength(100);
        });

        // MedicalRecord — store prescriptions as JSON
        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Diagnosis).HasMaxLength(500);
            e.Property(r => r.Symptoms).HasMaxLength(1000);
            e.Property(r => r.TreatmentPlan).HasMaxLength(1000);
            e.Property(r => r.PatientName).HasMaxLength(200);
            e.Property(r => r.DoctorName).HasMaxLength(200);
            e.Property(r => r.Prescriptions)
             .HasColumnType("jsonb");
        });

        // Bill — store items as JSON
        modelBuilder.Entity<Bill>(e =>
        {
            e.HasKey(b => b.Id);
            e.Property(b => b.PatientName).HasMaxLength(200);
            e.Property(b => b.Status).HasConversion<string>();
            e.Property(b => b.InsuranceProvider).HasMaxLength(200);
            e.Property(b => b.InsurancePolicyNumber).HasMaxLength(100);
            e.Property(b => b.Items)
             .HasColumnType("jsonb");
        });
    }
}
