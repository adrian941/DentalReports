using DentalReports.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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


    [HttpGet]
   
    public async Task<ActionResult<List<DisplayTechnician>>> GetTechnicians()
    {
        var technicians = await _userManager.GetUsersInRoleAsync(RolesMegagen.Technician);
        var displayTechnicians = technicians.Select(t => new DisplayTechnician
        {
            FirstName = t.FirstName,
            LastName = t.LastName ,
            Email = t.UserName!
        }).ToList();

        return Ok(displayTechnicians);
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



}
