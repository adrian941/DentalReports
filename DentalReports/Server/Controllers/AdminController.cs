using DentalReports.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalReports.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]

[Authorize(Roles = RolesMegagen.Admin)]
public class AdminController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _configuration;

    public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext, IConfiguration configuration)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _configuration = configuration;
    }


  

    [HttpPost]
     public async Task<ActionResult> AddTechnician(DisplayTechnician displayTechnician)
    {
        string Email = displayTechnician.Email!;
        ApplicationUser? technicianUser = _userManager.FindByEmailAsync(Email).Result;

        if(technicianUser == null)
        {
            return BadRequest($"User {Email} does not exist");
        }

        List<string> roles = (await _userManager.GetRolesAsync(technicianUser)).ToList() ;

        if(roles.Contains( RolesMegagen.Admin))
        {
            return BadRequest("An Admin cannot be a technician!");
        }
        if (roles.Contains(RolesMegagen.Doctor))
        {
            return BadRequest("A doctor cannot be a technician!");
        }
        if (!roles.Contains(RolesMegagen.Technician))
        {
            await _userManager.AddToRoleAsync(technicianUser, RolesMegagen.Technician);
        }

        if(_dbContext.Technicians.Any(t => t.Email.ToUpper().Trim() == Email.ToUpper().Trim() ))
        {
               return BadRequest($"Technician {technicianUser.FirstName} {technicianUser.LastName} already exists!");
        }

        Technician newTechnician = new Technician
        {
            Email = technicianUser!.Email!,
            sizeStoragePlanGB = 50,
            StoragePlan = StoragePlansTechnicians.BasicTechnician
        };

        //Adding to the tehnician the tutorial doctor (from _dbContext):  tom.scott@gmail.com
        Doctor tutorialDoctor = _dbContext.Doctors.FirstOrDefault(d => d.Email.ToUpper().Trim() == "tom.scott@gmail.com")!;
      
     
        List<PatientFile> patientfiles =
        [
            new PatientFile { Name = $"_STL_Config.json", sizeBytes = 0 },
            new PatientFile { Name = $"_Video_Config.txt", sizeBytes = 0 },
            new PatientFile { Name = $"Report_1.pdf", sizeBytes = 0 },
            new PatientFile { Name = $"STL_1.stl", sizeBytes = 0 },
            new PatientFile { Name = $"STL_2.stl", sizeBytes = 0 },
            new PatientFile { Name = $"STL_3.stl", sizeBytes = 0 },
            new PatientFile { Name = $"STL_4.stl", sizeBytes = 0 },
            new PatientFile { Name = $"STL_5.stl", sizeBytes = 0 },
            new PatientFile { Name = $"STL_6.stl", sizeBytes = 0 },
            new PatientFile { Name = $"Video_Final.mp4", sizeBytes = 0 },
        ];

        //Creating a new Patient without the PatientFiles
    


        //Adding the new patient to the technician

        newTechnician.Doctors.Add(tutorialDoctor);
        _dbContext.Technicians.Add(newTechnician);


        await _dbContext.SaveChangesAsync();



        Technician fetchedNewTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email.ToUpper().Trim() == Email.ToUpper().Trim())!;

        Patient newPatient = new Patient
        {
            DoctorId = tutorialDoctor.Id,    //Foreign key
            TechnicianId = fetchedNewTechnician.Id, //Foreign key
            FirstName = "Alex",
            LastName = "White",
            DateAdded = new DateTime(2022, 11, 23),
            PatientFiles = patientfiles
        };




        await _dbContext.SaveChangesAsync();

        _dbContext.Patients.Add(newPatient);
 
  

        await _dbContext.SaveChangesAsync();


 
        return Ok($"Technician {technicianUser.FirstName} {technicianUser.LastName} added as technician!");

         
    }


    [HttpGet]
    [Route("/api/admin/getPatient/{PatientId}")]
    public async Task<ActionResult<DisplayPatient>> getPatient(int PatientId)
    {
        Patient? patient =
            _dbContext.Patients.Include(p => p.PatientFiles)
            .Where(p => p.Id == PatientId).FirstOrDefault();
        if (patient == null)
        {
           return BadRequest($"Invalid Patient Id");
        }

        Technician technician = _dbContext.Technicians.FirstOrDefault(t => t.Id == patient.TechnicianId)!;
        Doctor doctor = _dbContext.Doctors.FirstOrDefault(d => d.Id == patient.DoctorId)!;
        ApplicationUser technicianUser = _userManager.FindByEmailAsync(technician.Email).Result!;
        ApplicationUser doctorUser = _userManager.FindByEmailAsync(doctor.Email).Result!;

        List<DisplayPatientFile> files = patient.PatientFiles.Select(pf => new DisplayPatientFile
        {
            Id = pf.Id,
            Name = pf.Name,
            SizeBytes = pf.sizeBytes
        }).ToList();

        bool hasPdf = files.Any(file => file.Name.Trim().ToLower().EndsWith(".pdf"));
        bool hasVideo = files.Any(file => file.Name.Trim().ToLower().EndsWith(".mp4"));
        bool hasStl = files.Any(file => file.Name.Trim().ToLower().EndsWith(".stl"));

        DisplayPatient displayPatient = new DisplayPatient
        {
            Id = patient.Id,
            TechnicianFirstName = technicianUser!.FirstName,
            TechnicianLastName = technicianUser!.LastName,
            TechnicianEmail = technicianUser!.Email!,
            DoctorFirstName = doctorUser!.FirstName,
            DoctorLastName = doctorUser!.LastName,
            DoctorEmail = doctorUser!.Email!,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateAdded = patient.DateAdded,
            Files = files,
            HasPdf = hasPdf,
            HasVideo = hasVideo,
            HasStl = hasStl
        };

        return Ok(displayPatient);



    }

    [HttpGet]
    public async Task<ActionResult<List<DisplayPatient>>> getPatients()
    {
        List<DisplayPatient> displayPatients = new List<DisplayPatient>();

        List<Patient> patients = await _dbContext.Patients.Include(p => p.PatientFiles).ToListAsync();
        
        foreach(Patient patient in patients)
        {
            List<PatientFile> files = patient.PatientFiles.ToList();

            bool hasPdf = files.Any(file => file.Name.Trim().ToLower().EndsWith(".pdf"));
            bool hasVideo = files.Any(file => file.Name.Trim().ToLower().EndsWith(".mp4"));
            bool hasStl = files.Any(file => file.Name.Trim().ToLower().EndsWith(".stl"));

            Technician  technician = _dbContext.Technicians.FirstOrDefault(t => t.Id == patient.TechnicianId)!;
            Doctor doctor = _dbContext.Doctors.FirstOrDefault(d => d.Id == patient.DoctorId)!;
            ApplicationUser technicianUser = _userManager.FindByEmailAsync(technician.Email).Result!;
            ApplicationUser doctorUser = _userManager.FindByEmailAsync(doctor.Email).Result!;

            DisplayPatient displayPatient = new DisplayPatient
            {
                Id = patient.Id,
                TechnicianFirstName = technicianUser!.FirstName,
                TechnicianLastName = technicianUser!.LastName,
                TechnicianEmail = technicianUser!.Email!,
                DoctorFirstName = doctorUser!.FirstName,
                DoctorLastName = doctorUser!.LastName,
                DoctorEmail = doctorUser!.Email!,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateAdded = patient.DateAdded,
                Files = patient.PatientFiles.Select(f => new DisplayPatientFile
                {
                    Name = f.Name,
                    SizeBytes = f.sizeBytes
                }).ToList(),
                HasPdf = hasPdf,
                HasVideo = hasVideo,
                HasStl = hasStl
            };


            displayPatients.Add(displayPatient);

        }








        return Ok(displayPatients);
    }

    [HttpGet]
    public async Task<ActionResult<List<DisplayDoctor>>> getDoctors()
    {
        List<Doctor> doctors = await _dbContext.Doctors.Include(d => d.Patients).ThenInclude(p => p.PatientFiles).ToListAsync();


        var displayDoctors = doctors.Select(d => new DisplayDoctor
        {
            FirstName = _userManager.FindByEmailAsync(d.Email).Result!.FirstName,
            LastName = _userManager.FindByEmailAsync(d.Email).Result!.LastName,
            Email = d.Email!,
            Id = d.Id
        }).ToList();

        return Ok(displayDoctors);
    }


    //[HttpGet]

    //public async Task<ActionResult<List<DisplayTechnician>>> GetTechnicians()
    //{
    //    var technicians = await _userManager.GetUsersInRoleAsync(RolesMegagen.Technician);
    //    var displayTechnicians = technicians.Select(t => new DisplayTechnician
    //    {
    //        FirstName = t.FirstName,
    //        LastName = t.LastName,
    //        Email = t.UserName!
    //    }).ToList();

    //    return Ok(displayTechnicians);
    //}
    [HttpGet]
    public async Task<ActionResult<List<DisplayTechnician>>> getTechnicians()
    {
        List<Technician> technicians = await _dbContext.Technicians.ToListAsync();
        var displayTechnicians = technicians.Select(t => new DisplayTechnician
        {

            FirstName = _userManager.FindByEmailAsync(t.Email).Result!.FirstName,
            LastName = _userManager.FindByEmailAsync(t.Email).Result!.LastName,
            Email = t.Email!,
            Id = t.Id
        }).ToList();

        return Ok(displayTechnicians);

    }

}
