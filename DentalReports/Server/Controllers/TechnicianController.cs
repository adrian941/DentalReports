using Azure.Storage.Blobs;
using DentalReports.Server.Data;
using DentalReports.Shared.Models;
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



    [HttpGet]
    public async Task<ActionResult<List<DisplayDoctor>>> getDoctors()
    {
        ApplicationUser? currentUser = await _userManager.GetUserAsync(User);
   
        Technician? currentTechnician = _dbContext.Technicians
            .Where(t => t.Email.ToUpper().Trim() == currentUser!.Email!.ToUpper().Trim()).FirstOrDefault();

        List<Doctor> doctors = currentTechnician!.Doctors.ToList();

        var displayDoctors = doctors.Select(d => new DisplayDoctor
        {
            FirstName = _userManager.FindByEmailAsync(d.Email).Result!.FirstName,
            LastName = _userManager.FindByEmailAsync(d.Email).Result!.LastName,
            Email = d.Email!,
            Id = d.Id
        }).ToList();

        //exclude from list the doctor with email = "tom.scott@gmail.com"

       


        return Ok(displayDoctors);
    }


    [HttpPost]
    public async Task<ActionResult> AddDoctor(DisplayDoctor displayDoctor)
    {
        string Email = displayDoctor.Email!;
        ApplicationUser? requestedUser = _userManager.FindByEmailAsync(Email).Result;
        ApplicationUser? currentLoggedUser = await _userManager.GetUserAsync(User);
        Technician? currentTechnician = _dbContext.Technicians
                .Where(t => t.Email == currentLoggedUser!.Email)
                .FirstOrDefault();

        if (requestedUser == null)
        {
            return BadRequest($"User '{Email}' does not exist");
        }
        List<string> roles = (await _userManager.GetRolesAsync(requestedUser)).ToList();

        if (roles.Contains(RolesMegagen.Admin))
        {
            return BadRequest("An Admin cannot be a doctor!");
        }

        if (roles.Contains(RolesMegagen.Technician))
        {
            return BadRequest("A Technician cannot be a doctor!");
        }


        await _userManager.AddToRoleAsync(requestedUser, RolesMegagen.Doctor);
 

        Doctor doctor = await _dbContext.Doctors
                        .Where(d => d.Email == requestedUser.Email)
                        .FirstOrDefaultAsync() 
                        ?? 
                        new Doctor
                        {
                            Email = requestedUser.Email!,
                        };
        if (currentTechnician!.Doctors.Contains(doctor))
        {
            return BadRequest($"Doctor {requestedUser.FirstName} {requestedUser.LastName} already exists in your Doctor list!");
        }
        
        currentTechnician!.Doctors.Add(doctor);
        await _dbContext.SaveChangesAsync();


        //Adding to the doctor the tutorial technician (from _dbContext):  emma.lee@gmail.com

        Technician tutorialTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email.ToUpper().Trim() == "emma.lee@gmail.com")!;

        Doctor fetchedNewDoctor =  _dbContext.Doctors.FirstOrDefault(d => d.Email.ToUpper().Trim() == requestedUser.Email.ToUpper().Trim())!;

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

        Patient newPatient = new Patient
        {
            DoctorId = fetchedNewDoctor.Id,    //Foreign key
            TechnicianId = tutorialTechnician.Id, //Foreign key
            FirstName = "Alex",
            LastName = "White",
            DateAdded = new DateTime(2022, 11, 23),
            PatientFiles = patientfiles
        };

        _dbContext.Patients.Add(newPatient);
        await _dbContext.SaveChangesAsync();












        return Ok($"Doctor {requestedUser.FirstName} {requestedUser.LastName} added to your Doctor list!");

    }
    
    [HttpPost]
    public async Task<ActionResult> AddPatient([FromForm] List<IFormFile> files, [FromForm] string patientJson)
    {
        FileErrors = string.Empty;
        if (!areFilesValid(files))
        {
            return BadRequest("Controller: " + FileErrors);
        }
       

        DisplayAddPatient displayAddPatient = JsonSerializer.Deserialize<DisplayAddPatient>(patientJson)!;
           
        ApplicationUser currentUser = ( await _userManager.GetUserAsync(User) )!;

        Technician? currentTechnician = _dbContext.Technicians.Where(t => t.Email.ToUpper().Trim() == currentUser.Email!.ToUpper().Trim()).FirstOrDefault();
        
   

        
        int doctorId = displayAddPatient.SelectedDoctorId;
        Doctor doctor = (await _dbContext.Doctors.Where(d => (d.Id == doctorId)).FirstOrDefaultAsync() )!;
        if (doctor == null)
        {
            return BadRequest($"Invalid Doctor !!");
        }
        bool isDoctorAssignedToTechnician = doctor.Technicians.Contains(currentTechnician!);
        if (!isDoctorAssignedToTechnician)
        {
            return BadRequest($"Invalid Doctor !!");
        }

        if(doctor.Email.ToLower().Trim() == "tom.scott@gmail.com")
        {
            return BadRequest($"You cannot add a patient to the tutorial doctor!");
        }



        bool patientExists = await _dbContext.Patients.AnyAsync(p => (p.TechnicianId == currentTechnician.Id && p.DoctorId == displayAddPatient.SelectedDoctorId && p.DateAdded == displayAddPatient.DateAdded && p.FirstName == displayAddPatient.FirstName
                                  && p.LastName == displayAddPatient.LastName));
        if (patientExists)
        {
            return BadRequest($"Patient {displayAddPatient.FirstName} {displayAddPatient.LastName} already exists at this Date!");
        }


        // ADD PATIENT
        int lastPatientId = 0;
        try
        {
            lastPatientId = _dbContext.Patients.OrderByDescending(p => p.Id).Select(p => p.Id).FirstOrDefault();
        }
        catch (Exception)
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
            DoctorId = displayAddPatient.SelectedDoctorId,
            FirstName = displayAddPatient.FirstName,
            LastName = displayAddPatient.LastName,
            DateAdded = displayAddPatient.DateAdded,
            PatientFiles = newFileNames.Select(newFileName => new PatientFile { Name = newFileName.FileName ,sizeBytes=newFileName.sizeBytes }).ToList(),
            
        };
        try
        {
            await _dbContext.Patients.AddAsync(patient);
        }
        catch (Exception)
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

    [HttpGet]
    public async Task<ActionResult<double>> getTotalPatientsSizeGB()
    {
        ApplicationUser currentUser = ( await _userManager.GetUserAsync(User) )!;
        Technician currentTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email == currentUser.Email)!;

        long totalSizeBytes = currentTechnician.Patients.SelectMany(p => p.PatientFiles).Sum(pf => pf.sizeBytes);
        double totalSizeGB = Math.Round((double)totalSizeBytes / (1024 * 1024 * 1024), 2);
        return Ok(totalSizeGB);
    }

    [HttpGet]
    public async Task<ActionResult<double>> getStoragePlanGB()
    {
        ApplicationUser currentUser = ( await _userManager.GetUserAsync(User) )!;
        Technician currentTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email == currentUser.Email)!;

        double storagePlanGB = currentTechnician.sizeStoragePlanGB;
        return Ok(storagePlanGB);

    }


        [HttpGet]
    public async Task<ActionResult<List<DisplayPatient>>> getPatients()
    {
        List<DisplayPatient> displayPatients = new List<DisplayPatient>();

        ApplicationUser currentTechnicianUser = ( await _userManager.GetUserAsync(User) )!;
        Technician currentTechnician = new();
        try {  currentTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email == currentTechnicianUser.Email)!; }
        catch (Exception )
        {
           
        }
        
        string technicianFirstName = currentTechnicianUser.FirstName;
        string technicianLastName = currentTechnicianUser.LastName;
        List<Patient> patients = _dbContext.Patients.Where(p => p.TechnicianId == currentTechnician.Id).ToList();

        foreach (Patient patient in patients)
        {
            Doctor doctor = _dbContext.Doctors.FirstOrDefault(d => d.Id == patient.DoctorId)!;
            ApplicationUser doctorUser = _dbContext.Users.FirstOrDefault(u => u.Email == doctor.Email)!;

            string doctorFirstName = doctorUser.FirstName;
            string doctorLastName = doctorUser.LastName;

            DateTime DateAdded = patient.DateAdded;
            List<DisplayPatientFile> files = patient.PatientFiles.Select(pf => new DisplayPatientFile
            {
                Id = pf.Id,
                Name = pf.Name,
                SizeBytes = pf.sizeBytes
            }).ToList();

            long totalFilesSizesBytes = patient.PatientFiles.Sum(pf => pf.sizeBytes);
            //get total size of files in MB (two decimals)
            double totalFilesSizesMB= Math.Round((double)totalFilesSizesBytes / (1024 * 1024), 1);

            bool hasPdf = files.Any(file => file.Name.Trim().ToLower().EndsWith(".pdf"));
            bool hasVideo = files.Any(file => file.Name.Trim().ToLower().EndsWith(".mp4"));
            bool hasStl = files.Any(file => file.Name.Trim().ToLower().EndsWith(".stl"));

            bool isForTutorial =  doctor.Email.ToLower().Trim() == "tom.scott@gmail.com" ? true : false;
            

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
                TotalFilesSizesMB = totalFilesSizesMB,
                HasPdf = hasPdf,
                HasVideo = hasVideo,
                HasStl = hasStl,
                IsForTutorial = isForTutorial

            });

        }
        return Ok(displayPatients);    

    }


    [HttpDelete]
    [Route("/api/technician/deletePatient/{PatientId}")]
    public async Task<ActionResult<DisplayPatient>> deletePatient(int PatientId)
    {
        Patient? patient = _dbContext.Patients.Where(p => p.Id == PatientId).FirstOrDefault();
        if (patient == null)
        {
            return BadRequest("Invalid Patient Id!");
        }
        ApplicationUser currentTechnicianUser = (await _userManager.GetUserAsync(User))!;
        Technician currentTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email == currentTechnicianUser.Email)!;

        if (!currentTechnician.Patients.Contains(patient))
        {
            return BadRequest("Invalid Patient Id!");
        }

 
        

     
        if(patient.DoctorId ==(await _dbContext.Doctors.Where(d => d.Email.ToLower().Trim()== "tom.scott@gmail.com").FirstAsync()).Id)
        {
            
            return Ok("Patient Deleted!");
        }
        _dbContext.Patients.Remove(patient);


        // delete files from azure
        string azureConnString = _config.GetConnectionString("AzureStorage")!;
        string containerName = _config.GetValue<string>("AzureContainerName")!;
        foreach (var file in patient.PatientFiles)
        {
            string blobName = file.Name;
            var blobClient = new BlobClient(azureConnString, containerName, blobName);

            await blobClient.DeleteIfExistsAsync();
        }
        
        await _dbContext.SaveChangesAsync();
        return Ok("Patient Deleted!");





    }

        [HttpGet]
    [Route("/api/technician/getPatient/{PatientId}")]
    public async Task<ActionResult<DisplayPatient>> getPatient(int PatientId)
    {

        Patient? patient = _dbContext.Patients.Where(p => p.Id == PatientId).FirstOrDefault() ;
        if (patient == null)
        {
            return BadRequest("Invalid Patient Id!");
        }
 
        ApplicationUser currentTechnicianUser = (await _userManager.GetUserAsync(User))!;
        Technician currentTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email == currentTechnicianUser.Email)!;

        if(!currentTechnician.Patients.Contains(patient))
        {
            return BadRequest("Invalid Patient Id!");
        }


        Doctor doctor = _dbContext.Doctors.FirstOrDefault(d => d.Id == patient.DoctorId)!;
        ApplicationUser doctorUser = _dbContext.Users.FirstOrDefault(u => u.Email == doctor.Email)!;


        string technicianFirstName = currentTechnicianUser.FirstName;
        string technicianLastName = currentTechnicianUser.LastName;
        string doctorFirstName = doctorUser.FirstName;
        string doctorLastName = doctorUser.LastName;

        bool hasPdf = patient.PatientFiles.Any(file => file.Name.Trim().ToLower().EndsWith(".pdf"));
        bool hasVideo = patient.PatientFiles.Any(file => file.Name.Trim().ToLower().EndsWith(".mp4"));
        bool hasStl = patient.PatientFiles.Any(file => file.Name.Trim().ToLower().EndsWith(".stl"));
     
        DateTime DateAdded = patient.DateAdded;
            List<DisplayPatientFile> files = patient.PatientFiles.Select(pf => new DisplayPatientFile
            {
                Id = pf.Id,
                Name = pf.Name,
                SizeBytes = pf.sizeBytes
            }).ToList();

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
    public async Task<ActionResult<List<DisplayTechnician>>> getCurrentTechnician()
    {
        List<DisplayTechnician> displayTechnicians = new List<DisplayTechnician>();

        ApplicationUser currentTechnicianUser = ( await _userManager.GetUserAsync(User) )!;
        Technician currentTechnician = _dbContext.Technicians.FirstOrDefault(t => t.Email == currentTechnicianUser.Email!.ToLower().Trim())!;

        displayTechnicians.Add(new DisplayTechnician
        {
            Id = currentTechnician.Id,
            FirstName = currentTechnicianUser.FirstName,
            LastName = currentTechnicianUser.LastName,
            Email = currentTechnicianUser.Email,
        });

        return Ok(displayTechnicians);




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
        //if (!files.Any(file => file.FileName == "_STL_Config.json"))
        //{
        //    FileErrors = "Missing file _STL_Config.json!";
        //    return false;
        //}

        if (files.Any(file => file.FileName.Length > 40))
        {
            FileErrors = "One file name has more than 20 characters!";
            return false;
        }


        //if (!files.Any(file => file.FileName.ToLower().Contains(".stl")))
        //{
        //    FileErrors = "Missing STL files";
        //    return false;
        //}


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


        string stlRegex = @"^STL_\d{1,3}\.stl$";
        string pdfRegex = @"^Report_\d{1,3}\.pdf$";

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
