using DentalReports.Server.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace DentalReports.Server.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DbInitializerController : ControllerBase
{
    private readonly IConfiguration _config;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public DbInitializerController(IConfiguration config,
                               UserManager<ApplicationUser> userManager,
                               RoleManager<IdentityRole> roleManager)
    {
        _config = config;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    //          /api/DbInitializer/bdDjsgfd356a2d443gdfg4345gs4wf4hDd23jfd43fdnghfknl3
    [HttpGet("{key}")]
    public async Task<string> InitDb(string key)
    {
        if (key != "bdDjsgfd356a2d443gdfg4345gs4wf4hDd23jfd43fdnghfknl3")
        {
            return "ERROR!";
        }
        await SetRolesAsync();
        await SetAdminAsync();

        return "Roles Set Successfully";


    }
    private async Task SetRolesAsync()
    {
        string[] roles = { RolesMegagen.Admin, RolesMegagen.Technician, RolesMegagen.Doctor };

        foreach (var roleName in roles)
        {
            bool roleExist = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }
    }

    private async Task SetAdminAsync()
    {
        string adminEmail = _config.GetValue<string>("AdminUser")!;
        ApplicationUser adminUser = (await _userManager.FindByEmailAsync(adminEmail))!;

        if (adminUser != null)
        {
            await _userManager.AddToRoleAsync(adminUser, RolesMegagen.Admin);
        }
    }

}
