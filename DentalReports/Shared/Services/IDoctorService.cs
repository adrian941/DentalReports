using DentalReports.Shared.DisplayModels;

namespace DentalReports.Shared.Services
{
    public interface IDoctorService
    {
        List<DisplayDoctor> DisplayDoctors { get; set; }
    }
}