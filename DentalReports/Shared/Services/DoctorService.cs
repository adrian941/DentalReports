using DentalReports.Shared.DisplayModels;
using DentalReports.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DentalReports.Shared.Services;

public class DoctorService : IDoctorService
{
    public List<DisplayDoctor> DisplayDoctors { get; set; } = new List<DisplayDoctor>();

}
