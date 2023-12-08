using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DentalReports.Server.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(150)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(150)]
    public string LastName { get; set; } = string.Empty;
    
}
