using Blazored.Modal;
using DentalReports.Client.Common;
using DentalReports.Shared.Common;
using DentalReports.Shared.DisplayModels;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Net.Http.Json;

namespace DentalReports.Client.Pages;

public partial class Patients 
{
    //Table Data
    private List<DisplayPatient> OriginalPatients = new List<DisplayPatient>();
    private List<DisplayPatient> FilteredPatients = new List<DisplayPatient>();
    private List<DisplayPatient> FrontPatients = new List<DisplayPatient>();

    private List<DisplayDoctor> OriginalDoctors = new List<DisplayDoctor>();
    private List<DisplayDoctor> FilteredDoctors = new List<DisplayDoctor>();
    private List<DisplayTechnician> OriginalTechnicians = new List<DisplayTechnician>();
    private List<DisplayTechnician> FilteredTechnicians = new List<DisplayTechnician>();

    // Paginator Data 
    private string paginator_StringRange = "";  // display info in paginator like:
                                                // "showing 1-10 of 100 entries"

    private int paginator_TotalRows = 0;        // total number Patient Entries
    private int paginator_EntriesPerPage = 10;
    private int paginator_PagesNumber = 0;
    private int paginator_CurrentPage = 1;
    private int selectedRow = -1; // Selected Row in Table

    // Sortings 
    private string DoctorSortingMode = SortingMode.None;
    private string TechnicianSortingMode = SortingMode.None;
    private string PatientSortingMode = SortingMode.None;
    private string DateSortingMode = SortingMode.None;

    // Search-Table Filters
    private string PatientNameFilter = "";
    private string DisplayDoctorTextFilters = "Select Doctors";
    private string DisplayTechnicianTextFilters = "Select Technicians";
    private MarkupString DisplayDateFilter = (MarkupString)$"Select Period";
    private DateTime FromDateFilter = DateTime.MinValue;
    private DateTime ToDateFilter = DateTime.MaxValue;

    private double totalPatientsSizeGB = 0;
    private double sizeStoragePlanGB = 0;

    private string role = "";

  



    
     



    public class SortingMode
    {
        public const string None = "None";
        public const string Ascending = "Ascending";
        public const string Descending = "Descending";
    }

}
