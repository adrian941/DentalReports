using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.DisplayModels;

public class DisplayPatient
{
    [Required(ErrorMessage = "The First Name Field is required")]
    [StringLength(50, ErrorMessage = "Max lenght is 50 characters.")]
    [MinLength(3, ErrorMessage = "Min lenght is 3 characters.")]
    public string FirstName { get; set; } = string.Empty;



    [Required(ErrorMessage = "The Last Name Field is required")]
    [StringLength(50, ErrorMessage = "Max lenght is 50 characters.")]
    [MinLength(3, ErrorMessage = "Min lenght is 3 characters.")]
    public string LastName { get; set; } = string.Empty;




    [Range(1, int.MaxValue, ErrorMessage = "Please choose a Doctor")]
    public int SelectedDoctorId { get; set; } = 0;
    public List<DisplayDoctor> Doctors { get; set; } = new();

    [Required(ErrorMessage = "The Date Added Field is required")]
    [Range(typeof(DateTime), "2010-01-01", "2500-12-31", ErrorMessage = "Please enter a valid Date!")]
    public DateTime DateAdded { get; set; } = new DateTime(2001, 1, 31);

    [Range(1, 20, ErrorMessage = "Please select the files! (Maximum: 20)")]
    public int FileCount { get; set; } = 0;




    //TODO : Verificari in Back-end IN CONTROLLERE pt lungimea string-urilor TOATE 
}
