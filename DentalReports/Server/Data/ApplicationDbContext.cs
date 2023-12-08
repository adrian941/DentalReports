using DentalReports.Server.Models;
using Duende.IdentityServer.EntityFramework.Options;
using Microsoft.AspNetCore.ApiAuthorization.IdentityServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace DentalReports.Server.Data;

public class ApplicationDbContext : ApiAuthorizationDbContext<ApplicationUser>
{
    public DbSet<Technician> Technicians => Set<Technician>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<PatientFile> PatientFiles => Set<PatientFile>();



    public ApplicationDbContext(
        DbContextOptions options,
        IOptions<OperationalStoreOptions> operationalStoreOptions) : base(options, operationalStoreOptions)
    {
 
    }


    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        //ApplicationUser
        builder.Entity<ApplicationUser>()
                .HasIndex(u => u.FirstName)
                .IsUnique(false);
                    builder.Entity<ApplicationUser>()
                        .HasIndex(u => u.LastName)
                        .IsUnique(false);

        //Doctor and Technician
        builder.Entity<Doctor>().Navigation(d => d.Patients).AutoInclude();
        builder.Entity<Doctor>()
         .HasMany(d => d.Technicians)
         .WithMany(t => t.Doctors)
         .UsingEntity(j =>
             {
                 j.ToTable("DoctorsTechnicians");
                 j.Property<int>("DoctorId");
                 j.Property<int>("TechnicianId");
                 j.HasKey("DoctorId", "TechnicianId");
             }
         );
        builder.Entity<Doctor>()
         .HasIndex(e => e.Email)
         .IsUnique(true);
        builder.Entity<Technician>()
         .HasIndex(t => t.Email)
         .IsUnique(true);

        builder.Entity<Technician>().Navigation(t => t.Doctors).AutoInclude();


        //Patient
        builder.Entity<Patient>()
             .HasIndex(p => new { p.LastName })
             .IsUnique(false);

        builder.Entity<Patient>()
            .HasIndex(p => new { p.FirstName })
            .IsUnique(false);


        builder.Entity<Patient>()
            .HasIndex(p => new { p.DateAdded, p.FirstName, p.LastName, p.DoctorId, p.TechnicianId })
            .IsUnique(true);

        builder.Entity<Patient>().Navigation(p => p.PatientFiles).AutoInclude();





    }

}
