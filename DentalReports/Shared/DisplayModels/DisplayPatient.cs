using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.DisplayModels;

public class DisplayPatient
{
    public int Id { get; set; } = -1;

    public string TechnicianFirstName { get; set; } = string.Empty;
    public string TechnicianLastName { get; set; } = string.Empty;
    public string DoctorFirstName { get; set; } = string.Empty;
    public string DoctorLastName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public DateTime DateAdded { get; set; } = DateTime.MinValue;

    public List<DisplayPatientFile> Files { get; set; } = new List<DisplayPatientFile>();

}
