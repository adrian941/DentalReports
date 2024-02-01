using DentalReports.Server.Data;
using Duende.IdentityServer.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DentalReports.Server.Controllers; 

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = RolesMegagen.Doctor)]
public class DoctorController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;

    public DoctorController(UserManager<ApplicationUser> userManager,
                                ApplicationDbContext dbContext,
                                HttpClient httpClient)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _httpClient = httpClient;
    }


    [HttpGet]
    public async Task<ActionResult<List<DisplayPatient>>> getPatients()
    {
        List<DisplayPatient> displayPatients = new List<DisplayPatient>();

        ApplicationUser currentDoctorUser = ( await _userManager.GetUserAsync(User) )!;
        Doctor currentDoctor = _dbContext.Doctors.FirstOrDefault(d => d.Email == currentDoctorUser.Email)!;
        string doctorFirstName = currentDoctorUser.FirstName;
        string doctorLastName = currentDoctorUser.LastName;
 
        List<Patient> patients = 
            _dbContext.Patients
            .Include(p => p.PatientFiles)
            .Where(p => p.DoctorId == currentDoctor.Id).ToList();





        foreach (Patient patient in patients)
        {
            Technician technician = _dbContext.Technicians.FirstOrDefault(t => t.Id == patient.TechnicianId)!;
            ApplicationUser technicianUser = _dbContext.Users.FirstOrDefault(u => u.Email == technician.Email)!;
            
            string technicianFirstName = technicianUser.FirstName;
            string technicianLastName = technicianUser.LastName;
            
            DateTime DateAdded = patient.DateAdded;
            List<DisplayPatientFile> files = patient.PatientFiles.Select(pf => new DisplayPatientFile
            {
                Id = pf.Id,
                Name = pf.Name,
                SizeBytes = pf.sizeBytes
            }).ToList();

            bool hasPdf = files.Any(f => f.Name.Trim().ToLower().EndsWith(".pdf"));
            bool hasVideo = files.Any(f => f.Name.Trim().ToLower().EndsWith(".mp4"));
            bool hasStl = files.Any(f => f.Name.Trim().ToLower().EndsWith(".stl"));

            displayPatients.Add( new DisplayPatient
            {
                Id = patient.Id,
                TechnicianFirstName = technicianFirstName,
                TechnicianLastName = technicianLastName,
                DoctorFirstName = doctorFirstName,
                DoctorLastName = doctorLastName,
                FirstName = patient.FirstName,
                LastName = patient.LastName,
                DateAdded = DateAdded,
                Files = files,
                HasPdf = hasPdf,
                HasVideo = hasVideo,
                HasStl = hasStl
            });
            
        }

        return Ok(displayPatients);
    }

    [HttpGet]
    [Route("/api/doctor/getPatient/{PatientId}")]
    public async Task<ActionResult<List<DisplayPatient>>> getPatient(int PatientId)
    {
        Patient? patient = 
            _dbContext.Patients.Include(p => p.PatientFiles)
            .Where(p => p.Id == PatientId).FirstOrDefault();
        if (patient == null)
        {
            return BadRequest("Invalid Patient Id!");
        }


        ApplicationUser currentDoctorUser = (await _userManager.GetUserAsync(User))!;
        Doctor currentDoctor = _dbContext.Doctors.FirstOrDefault(d => d.Email == currentDoctorUser.Email)!;

        if (!currentDoctor.Patients.Contains(patient))
        {
            return BadRequest("Invalid Patient Id!");
        }


        Technician technician = _dbContext.Technicians.FirstOrDefault(t => t.Id == patient.TechnicianId)!;
        ApplicationUser technicianUser = _dbContext.Users.FirstOrDefault(u => u.Email == technician.Email)!;


        string technicianFirstName = technicianUser.FirstName;
        string technicianLastName = technicianUser.LastName;
        string doctorFirstName = currentDoctorUser.FirstName;
        string doctorLastName = currentDoctorUser.LastName;

        DateTime DateAdded = patient.DateAdded;
        List<DisplayPatientFile> files = patient.PatientFiles.Select(pf => new DisplayPatientFile
        {
            Id = pf.Id,
            Name = pf.Name,
            SizeBytes = pf.sizeBytes
        }).ToList();

        bool hasPdf = files.Any(f => f.Name.Trim().ToLower().EndsWith(".pdf"));
        bool hasVideo = files.Any(f => f.Name.Trim().ToLower().EndsWith(".mp4"));
        bool hasStl = files.Any(f => f.Name.Trim().ToLower().EndsWith(".stl"));

        DisplayPatient displayPatient = new DisplayPatient
        {
            Id = patient.Id,
            TechnicianFirstName = technicianFirstName,
            TechnicianLastName = technicianLastName,
            DoctorFirstName = doctorFirstName,
            DoctorLastName = doctorLastName,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateAdded = DateAdded,
            Files = files,
            HasPdf = hasPdf,
            HasVideo = hasVideo,
            HasStl = hasStl
        };

        return Ok(displayPatient);



    }

    [HttpGet] 
    public async Task<ActionResult<List<DisplayDoctor>>> getCurrentDoctor()
    { 

        List<DisplayDoctor> displayDoctor = new List<DisplayDoctor>();

        ApplicationUser currentDoctorUser = ( await _userManager.GetUserAsync(User) )!;
        Doctor currentDoctor = _dbContext.Doctors.FirstOrDefault(d => d.Email.ToLower().Trim() == currentDoctorUser.Email!.ToLower().Trim())!;

        displayDoctor.Add( new DisplayDoctor
        {
            Id = currentDoctor.Id,
            FirstName = currentDoctorUser.FirstName,
            LastName = currentDoctorUser.LastName,
            Email = currentDoctorUser.Email
        });

        return Ok(displayDoctor);        
 
    }

    [HttpGet]
    public async Task<ActionResult<List<DisplayDoctor>>> getTechnicians()
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return BadRequest("User does not exist");
        }
        Doctor? currentDoctor = null;
       
        try
        {
            
            currentDoctor = _dbContext.Doctors
                .Include(d => d.Technicians)
                .Where(d => d.Email.ToLower().Trim() == currentUser.Email!.ToLower().Trim())
                .FirstOrDefault();

           
       
            

             
        }
         catch (Exception ex)
        {
            
        }

        List<Technician> technicians = currentDoctor!.Technicians.ToList();

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
