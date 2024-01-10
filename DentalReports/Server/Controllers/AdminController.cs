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

    public AdminController(UserManager<ApplicationUser> userManager, ApplicationDbContext dbContext)
    {
        _userManager = userManager;
        _dbContext = dbContext;
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
        };

        _dbContext.Technicians.Add(newTechnician);
        await _dbContext.SaveChangesAsync();


 
        return Ok($"Technician {technicianUser.FirstName} {technicianUser.LastName} added as technician!");

         
    }



}
