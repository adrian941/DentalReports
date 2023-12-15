using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.Models;

public class Technician
{
    public int Id { get; set; }

    [Required]
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = "";

    public List<Doctor> Doctors { get; set; } = new List<Doctor>();
    public List<Patient> Patients { get; set; } = new List<Patient>();

    public int sizeStoragePlanGB { get; set; } = 50;
}
