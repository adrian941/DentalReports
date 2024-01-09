using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.DisplayModels;

public class DisplayDoctor
{
    public int Id { get; set; } = 0;

    public string? FirstName { get; set; } = "";
    public string? LastName { get; set; } = "";

    [EmailAddress]
    public string? Email { get; set; } = "";

  
    public DisplayDoctor()
    {
        Id = 0;
        FirstName = "";
        LastName = "";
        Email = "";
    }

}
