using Blazored.Modal;
using DentalReports.Client.Common;
using DentalReports.Shared.Common;
using DentalReports.Shared.DisplayModels;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;

namespace DentalReports.Client.Pages;

public partial class Patients
{
    protected async Task NavigateToAddPatient()
    {

        ModalParameters parameters = new ModalParameters();

        parameters.Add("totalPatientsSizeGB", totalPatientsSizeGB);
        parameters.Add("sizeStoragePlanGB", sizeStoragePlanGB);
        // parameters.Add("OriginalTechnicians", OriginalTechnicians);


        ModalOptions options = new ModalOptions();

        options.DisableBackgroundCancel = true;

        var modalWindows = _modal.Show<AddPatient>("Add new Patient", parameters, options);
        var result = await modalWindows.Result;

        if (role == RolesMegagen.Technician)
        {
            OriginalPatients = await _httpClient.GetFromJsonAsync<List<DisplayPatient>>("api/technician/getPatients") ?? new List<DisplayPatient>();
        }
        else if (role == RolesMegagen.Doctor)
        {
            OriginalPatients = await _httpClient.GetFromJsonAsync<List<DisplayPatient>>("api/doctor/getPatients") ?? new List<DisplayPatient>();
        }
        else
        {
            return;
        }



        FrontPatients = OriginalPatients;
        FilteredPatients = OriginalPatients;
        FilteredDoctors = OriginalDoctors;

        ApplyAllFilters();

        await CalculateStorageDetailsOfTechnician();

    }

    public async void ShowTrashModal(int PatientId)
    {

        ModalParameters parameters = new ModalParameters();

        parameters.Add("PatientId", PatientId);

        ModalOptions options = new ModalOptions();
        options.DisableBackgroundCancel = false;

        var modalWindow = _modal.Show<ModalTrash>("Delete confirmation!", parameters, options);
        var result = await modalWindow.Result;

        if (result.Cancelled)
        {
            return;
        }

        // refresh displayPatient from API 

        if (role == RolesMegagen.Technician)
        {
            OriginalPatients = await _httpClient.GetFromJsonAsync<List<DisplayPatient>>("api/technician/getPatients") ?? new List<DisplayPatient>();
        }
        else if (role == RolesMegagen.Doctor)
        {
            OriginalPatients = await _httpClient.GetFromJsonAsync<List<DisplayPatient>>("api/doctor/getPatients") ?? new List<DisplayPatient>();
        }
        else
        {
            return; // Let just the default HTML messages to be displayed
        }

        FrontPatients = OriginalPatients;
        FilteredPatients = OriginalPatients;
        FilteredDoctors = OriginalDoctors;

        ApplyAllFilters();

        await CalculateStorageDetailsOfTechnician();







    }

    
    //Filtering
    private async void OpenTechniciansModal()
    {
        ModalParameters parameters = new ModalParameters();
        parameters.Add("OriginalTechnicians", OriginalTechnicians);

        ModalOptions options = new ModalOptions();
        options.DisableBackgroundCancel = false;

        var modalWindows = _modal.Show<ModalTechniciansFilter>("Choose Technicians", parameters, options);
        var result = await modalWindows.Result;

        if (result.Cancelled)
        {
            return;
        }
        FilteredTechnicians = ((List<DisplayTechnician>)(result.Data!))!;

        if (FilteredTechnicians.Count > 1)
        {
            DisplayTechnicianTextFilters = $"{FilteredTechnicians.Count} Technicians selected";
        }
        else if (FilteredTechnicians.Count == 1)
        {
            DisplayTechnicianTextFilters = $"{FilteredTechnicians[0].FirstName} {FilteredTechnicians[0].LastName}";
        }
        else
        {
            DisplayTechnicianTextFilters = $"No Techs. selected";
        }
        await _localStorage.SetItemAsync<List<DisplayTechnician>>("filteredTechnicians", FilteredTechnicians);
        ApplyAllFilters();

    }
    private async void OpenDoctorsModal()
    {
        ModalParameters parameters = new ModalParameters();
        parameters.Add("OriginalDoctors", OriginalDoctors);

        ModalOptions options = new ModalOptions();
        options.DisableBackgroundCancel = false;

        var modalWindows = _modal.Show<ModalDoctorsFilter>("Choose Doctors", parameters, options);
        var result = await modalWindows.Result;

        if (result.Cancelled)
        {
            return;
        }
        FilteredDoctors = ((List<DisplayDoctor>)(result.Data!))!;

        if (FilteredDoctors.Count > 1)
        {
            DisplayDoctorTextFilters = $"{FilteredDoctors.Count} Doctors selected";
        }
        else if (FilteredDoctors.Count == 1)
        {
            DisplayDoctorTextFilters = $"{FilteredDoctors[0].FirstName} {FilteredDoctors[0].LastName}";
        }
        else
        {
            DisplayDoctorTextFilters = $"No Doctors selected";
        }

        await _localStorage.SetItemAsync<List<DisplayDoctor>>("filteredDoctors", FilteredDoctors);


        ApplyAllFilters();
    }
 
    private async void OpenDateModal()
    {
        ModalParameters parameters = new ModalParameters();
        parameters.Add("FromDate", FromDateFilter);
        parameters.Add("ToDate", ToDateFilter);

        ModalOptions options = new ModalOptions();
        options.DisableBackgroundCancel = false;

        var modalWindows = _modal.Show<ModalDateFilter>("Select period", parameters, options);
        var result = await modalWindows.Result;

        if (result.Cancelled)
        {
            return;
        }

        FromDateFilter = ((Dates)(result.Data!))!.FromDate;
        ToDateFilter = ((Dates)(result.Data!))!.ToDate;

        await _localStorage.SetItemAsync<DateTime>("fromDateFilter", FromDateFilter);
        await _localStorage.SetItemAsync<DateTime>("toDateFilter", ToDateFilter);




        DisplayDateFilter = (MarkupString)$"{MegagenDates.GetMegagenShortDate(FromDateFilter)} to <br /> {MegagenDates.GetMegagenShortDate(ToDateFilter)}";

        ApplyAllFilters();
    }
}
