using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.Models;

public class Patient
{
    public int Id { get; set; }

    [Required]
    [ForeignKey("DoctorId")]
    public int DoctorId { get; set; }

    [Required]
    [ForeignKey("TechnicianId")]
    public int TechnicianId { get; set; }



    [Required]
    [MaxLength(50)]

    public string FirstName { get; set; } = string.Empty;


    [Required]
    [MaxLength(50)]
    public string LastName { get; set; } = string.Empty;



    [Required]
    [Column(TypeName = "Date")]
    public DateTime DateAdded { get; set; }
    public List<PatientFile> PatientFiles { get; set; } = new List<PatientFile>();
}
