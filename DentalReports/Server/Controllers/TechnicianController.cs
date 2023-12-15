using Azure.Storage.Blobs;
using DentalReports.Server.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using static Duende.IdentityServer.Models.IdentityResources;

namespace DentalReports.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
[Authorize(Roles = RolesMegagen.Technician)]
public class TechnicianController : ControllerBase
{
    private string FileErrors = string.Empty;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;

    public TechnicianController(UserManager<ApplicationUser> userManager,
                                ApplicationDbContext dbContext, 
                                IConfiguration config,
                                HttpClient httpClient)
    {
        _userManager = userManager;
        _dbContext = dbContext;
        _config = config;
        _httpClient = httpClient;
    }
    public async Task<ActionResult<List<DisplayDoctor>>> GetDoctors()
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return BadRequest("User does not exist");
        }


        Technician? currentTechnician = _dbContext.Technicians
           .Where(t => t.Email == currentUser!.Email)
           .FirstOrDefault();

        List<Doctor> doctors = currentTechnician!.Doctors.ToList();

        var displayDoctors = doctors.Select(d => new DisplayDoctor
        {
            FirstName = _userManager.FindByEmailAsync(d.Email).Result!.FirstName,
            LastName = _userManager.FindByEmailAsync(d.Email).Result!.LastName,
            Email = d.Email!,
            Id = d.Id
        }).ToList();


        return Ok(displayDoctors);


    }

    [HttpPost]
    public async Task<ActionResult> AddDoctor(DisplayDoctor displayDoctor)
    {
        string Email = displayDoctor.Email!;
        ApplicationUser? doctorUser = _userManager.FindByEmailAsync(Email).Result;
        ApplicationUser? currentLoggedUser = await _userManager.GetUserAsync(User);
        Technician? currentTechnician = _dbContext.Technicians
                .Where(t => t.Email == currentLoggedUser!.Email)
                .FirstOrDefault();

        if (doctorUser == null)
        {
            return BadRequest($"Doctor with email {Email} does not exist");
        }
        List<string> roles = (await _userManager.GetRolesAsync(doctorUser)).ToList();

        if (roles.Contains(RolesMegagen.Admin))
        {
            return BadRequest("An Admin cannot be a doctor!");
        }

        if (roles.Contains(RolesMegagen.Technician))
        {
            return BadRequest("A Technician cannot be a doctor!");
        }


        await _userManager.AddToRoleAsync(doctorUser, RolesMegagen.Doctor);
        Doctor newDoctor = new Doctor
        {
            Email = doctorUser.Email!
        };

        currentTechnician!.Doctors.Add(newDoctor);
        await _dbContext.SaveChangesAsync();

        return Ok($"Doctor {doctorUser.FirstName} {doctorUser.LastName} added to your Doctor list!");

    }














    [HttpPost]
    public async Task<ActionResult> AddPatient([FromForm] List<IFormFile> files, [FromForm] string patientJson)
    {
        FileErrors = string.Empty;
        if (!areFilesValid(files))
        {
            return BadRequest("Controller: " + FileErrors);
        }

        DisplayPatient displayPatient = JsonSerializer.Deserialize<DisplayPatient>(patientJson)!;
           
        ApplicationUser currentUser = ( await _userManager.GetUserAsync(User) )!;

        Technician? currentTechnician = _dbContext.Technicians.Where(t => t.Email.ToUpper().Trim() == currentUser.Email!.ToUpper().Trim()).FirstOrDefault();
        int doctorId = displayPatient.SelectedDoctorId;
        Doctor doctor = (await _dbContext.Doctors.Where(d => (d.Id == doctorId)).FirstOrDefaultAsync() )!;
        if (doctor == null)
        {
            return BadRequest($"Invalid Doctor !!");
        }
        bool isDoctorAssignedToTechnician = doctor.Technicians.Contains(currentTechnician);
        if (!isDoctorAssignedToTechnician)
        {
            return BadRequest($"Invalid Doctor !!");
        }
        bool patientExists = await _dbContext.Patients.AnyAsync(p => (p.TechnicianId == currentTechnician.Id && p.DoctorId == displayPatient.SelectedDoctorId && p.DateAdded == displayPatient.DateAdded && p.FirstName == displayPatient.FirstName
                                  && p.LastName == displayPatient.LastName));
        if (patientExists)
        {
            return BadRequest($"Patient {displayPatient.FirstName} {displayPatient.LastName} already exists at this Date!");
        }


        // ADD PATIENT
        int lastPatientId = 0;
        try
        {
            lastPatientId = _dbContext.Patients.OrderByDescending(p => p.Id).Select(p => p.Id).FirstOrDefault();
        }
        catch (Exception ex)
        {

        }
        lastPatientId++;
        string sufixFile = "F" + DateTime.Now.ToString("yyyyMMddHHmmssf") + "_"
                             + StringPrelucration.GetRandomFileName(40, 50)
                             + $"_T{currentTechnician.Id}D{doctorId}P{lastPatientId}_";

        List<SendFileModel> newFileNames = new List<SendFileModel>();
        foreach (IFormFile file in files)
        {
            string fileName = sufixFile + file.FileName;
            int sizeBytes = (int)(file.Length);

            newFileNames.Add(new SendFileModel { FileName = fileName, sizeBytes = sizeBytes });
        }
            
            
            
            
            
            
            //files.Select(file => sufixFile + file.FileName).ToList();

        Patient patient = new Patient
        {
            TechnicianId = currentTechnician.Id,
            DoctorId = displayPatient.SelectedDoctorId,
            FirstName = displayPatient.FirstName,
            LastName = displayPatient.LastName,
            DateAdded = displayPatient.DateAdded,
            PatientFiles = newFileNames.Select(newFileName => new PatientFile { Name = newFileName.FileName ,sizeBytes=newFileName.sizeBytes }).ToList(),
            
        };
        try
        {
            await _dbContext.Patients.AddAsync(patient);
        }
        catch (Exception ex)
        {
            return BadRequest("Same Patient at same Date. Already Added!");
        }

        //SAVE FILES

        string azureConnString = _config.GetConnectionString("AzureStorage")!;
        string containerName =  _config.GetValue<string>("AzureContainerName")!;
        foreach (var file in files)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            // Set the position of the stream to 0
            memoryStream.Position = 0;

            string blobName = sufixFile + file.FileName;
            var blobClient = new BlobClient(azureConnString, containerName, blobName);

            await blobClient.UploadAsync(memoryStream);
        }




        _dbContext.SaveChanges();
        return Ok("Patient Saved!");

    }





















































    private bool areFilesValid(List<IFormFile> files)
    {

        int maxAllowedFiles = 20;
        int maxFileSize = 1024 * 1024 * 100; //TODO Vezi daca poti seta in Appsettings.json 100MB




        if (files.Count > maxAllowedFiles)
        {
            FileErrors = $"You can upload maximum {maxAllowedFiles} files!";
            return false;
        }


        if (files.Sum(file => file.Length) > maxFileSize)
        {
            FileErrors = $"The total size of the files should be less than {maxFileSize / (1024 * 1024)}MB!";
            return false;
        }

        // Check if a file with a specific name is here
        if (!files.Any(file => file.FileName == "_STL_Config.json"))
        {
            FileErrors = "Missing file _STL_Config.json!";
            return false;
        }

        if (files.Any(file => file.FileName.Length > 40))
        {
            FileErrors = "One file name has more than 20 characters!";
            return false;
        }


        if (!files.Any(file => file.FileName.ToLower().Contains(".stl")))
        {
            FileErrors = "Missing STL files";
            return false;
        }


        if (files.Any(file => file.FileName == "_Video_Config.txt"))
        {
            if (!files.Any(file => file.FileName == "Video_Final.mp4"))
            {
                FileErrors = "Missing file Video_Final.mp4";
                return false;
            }
        }

        if (files.Any(file => file.FileName == "Video_Final.mp4"))
        {
            if (!files.Any(file => file.FileName == "_Video_Config.txt"))
            {
                FileErrors = "Missing file _Video_Config.txt";
                return false;
            }
        }


        string stlRegex = @"^STL_[\w\-]+\.stl$";
        string pdfRegex = @"^Report_\d+\.pdf$";

        string[] filesNames = files.Select(file => file.FileName).ToArray();
        string fileNamesErrors = "File(s) : ";
        bool areFilesCorrect = true;
        foreach (string fileName in filesNames)
        {
            bool isStlMatch = Regex.IsMatch(fileName, stlRegex);
            bool isPdfMatch = Regex.IsMatch(fileName, pdfRegex);

            if (!(isStlMatch || isPdfMatch || fileName == "_STL_Config.json" || fileName == "_Video_Config.txt" || fileName == "Video_Final.mp4"))
            {
                fileNamesErrors += "'" + fileName + "', ";
                areFilesCorrect = false;
            }

        }
        fileNamesErrors = fileNamesErrors.Substring(0, fileNamesErrors.Length - 2);
        fileNamesErrors += "-> are not accepted ! Their name does not contain the default File Name!";
        if (!areFilesCorrect)
        {
            FileErrors = fileNamesErrors;
            return false;
        }

        return true;
    }

    public class SendFileModel
    {
       public string FileName { get; set; } = string.Empty;
       public int sizeBytes { get; set; } = 0;
    }

}
