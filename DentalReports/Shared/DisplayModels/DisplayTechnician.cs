using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.DisplayModels;

public class DisplayTechnician
{
    public string? FirstName { get; set; } = "";
    public string? LastName { get; set; } = "";

    [EmailAddress]
    public string? Email { get; set; } = "";
}
