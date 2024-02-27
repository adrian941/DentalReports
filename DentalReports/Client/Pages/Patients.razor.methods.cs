using DentalReports.Client.Common;
using DentalReports.Shared.DisplayModels;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;

namespace DentalReports.Client.Pages;

public partial class Patients
{
    protected override async Task OnInitializedAsync()
    {


        await FetchFromLocalStorageAsync();
        await FetchFromApiAsync();

        if (role != null && role == RolesMegagen.Technician)
        {
            await CalculateStorageDetailsOfTechnician();
        }


    }

    //Fetch data
    public async Task FetchFromLocalStorageAsync()
    {

        try
        {
            role = await _localStorage.GetItemAsync<string>("role") ?? "";
        }
        catch (Exception)
        {
            role = "";
        }

        //fetch OriginalDoctors from LocalStorage
        try
        {
            var doctors = await _localStorage.GetItemAsync<List<DisplayDoctor>>("doctors") ?? new List<DisplayDoctor>();
            var patients = await _localStorage.GetItemAsync<List<DisplayPatient>>("patients") ?? new List<DisplayPatient>();
            var technicians = await _localStorage.GetItemAsync<List<DisplayTechnician>>("technicians") ?? new List<DisplayTechnician>();
            var technicianSortingMode = await _localStorage.GetItemAsync<string>("technicianSortingMode") ?? SortingMode.None;

            var doctorSortingMode = await _localStorage.GetItemAsync<string>("doctorSortingMode") ?? SortingMode.None;
            var nameSortingMode = await _localStorage.GetItemAsync<string>("nameSortingMode") ?? SortingMode.None;
            var dateSortingMode = await _localStorage.GetItemAsync<string>("dateSortingMode") ?? SortingMode.None;

            var patientNameFilter = await _localStorage.GetItemAsync<string>("patientNameFilter") ?? null;
            var filteredDoctors = await _localStorage.GetItemAsync<List<DisplayDoctor>>("filteredDoctors") ?? null;
            var filteredTechnicians = await _localStorage.GetItemAsync<List<DisplayTechnician>>("filteredTechnicians") ?? null;

            DateTime fromDateFilter = DateTime.MinValue;
            DateTime toDateFilter = DateTime.MaxValue;
            try
            {
                fromDateFilter = await _localStorage.GetItemAsync<DateTime>("fromDateFilter");
                toDateFilter = await _localStorage.GetItemAsync<DateTime>("toDateFilter");
            }
            catch (Exception) { }


            OriginalPatients = patients;
            OriginalDoctors = doctors;
            OriginalTechnicians = technicians;

            //Sortings
            DoctorSortingMode = doctorSortingMode;
            TechnicianSortingMode = technicianSortingMode;
            PatientSortingMode = nameSortingMode;
            DateSortingMode = dateSortingMode;



            FrontPatients = OriginalPatients;
            FilteredPatients = OriginalPatients;

            PatientNameFilter = patientNameFilter ?? "";
            FilteredDoctors = filteredDoctors ?? OriginalDoctors;
            FilteredTechnicians = filteredTechnicians ?? OriginalTechnicians;




            await InitPaginatorAsync();
            StateHasChanged();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

        try
        {
            totalPatientsSizeGB = await _localStorage.GetItemAsync<double>("totalPatientsSizeGB");
            sizeStoragePlanGB = await _localStorage.GetItemAsync<double>("sizeStoragePlanGB");
        }
        catch (Exception)
        {

        }

    }
    public async Task FetchFromApiAsync()
    {
        List<DisplayPatient>? patients = null;
        List<DisplayDoctor>? doctors = null;
        List<DisplayTechnician>? technicians = null;



        //Get Role
        var User = (await _authenticationStateProvider.GetAuthenticationStateAsync()).User;

        if (role != User.Claims.FirstOrDefault(x => x.Type == "role")?.Value!)
        {
            role = User.Claims.FirstOrDefault(x => x.Type == "role")?.Value!;
            await _localStorage.SetItemAsync<string>("role", role);
        }


        //localStorage
        if (role == RolesMegagen.Technician)
        {
            try
            {
                patients = await _httpClient.GetFromJsonAsync<List<DisplayPatient>>("api/technician/getPatients") ?? new List<DisplayPatient>();
                doctors = await _httpClient.GetFromJsonAsync<List<DisplayDoctor>>("api/technician/getDoctors") ?? new List<DisplayDoctor>();
                technicians = await _httpClient.GetFromJsonAsync<List<DisplayTechnician>>("api/technician/getCurrentTechnician") ?? new List<DisplayTechnician>();
            }
            catch (Exception ex)
            {

                _navigationManager.NavigateToLogout("/authentication/logout");
            }

        }
        else if (role == RolesMegagen.Doctor)
        {
            try
            {
                patients = await _httpClient.GetFromJsonAsync<List<DisplayPatient>>("api/doctor/getPatients") ?? new List<DisplayPatient>();
                doctors = await _httpClient.GetFromJsonAsync<List<DisplayDoctor>>("api/doctor/getCurrentDoctor") ?? new List<DisplayDoctor>();
                technicians = await _httpClient.GetFromJsonAsync<List<DisplayTechnician>>("api/doctor/getTechnicians") ?? new List<DisplayTechnician>();
            }
            catch (Exception ex)
            {
                _navigationManager.NavigateToLogout("/authentication/logout");
            }
        }
        else if (role == RolesMegagen.Admin)
        {
            try
            {
                patients = await _httpClient.GetFromJsonAsync<List<DisplayPatient>>("api/admin/getPatients") ?? new List<DisplayPatient>();
                doctors = await _httpClient.GetFromJsonAsync<List<DisplayDoctor>>("api/admin/getDoctors") ?? new List<DisplayDoctor>();
                technicians = await _httpClient.GetFromJsonAsync<List<DisplayTechnician>>("api/admin/getTechnicians") ?? new List<DisplayTechnician>();
            }
            catch (Exception ex)
            {

                _navigationManager.NavigateToLogout("/authentication/logout");
            }
        }
        else
        {
            return;
        }








        //doctors
        if (!doctors.SequenceEqual(OriginalDoctors) || !patients.SequenceEqual(OriginalPatients))
        {
            OriginalPatients = patients;
            OriginalDoctors = doctors;
            OriginalTechnicians = technicians;

            FrontPatients = OriginalPatients;
            FilteredPatients = OriginalPatients;

            FilteredDoctors = OriginalDoctors;
            FilteredTechnicians = OriginalTechnicians;

            await InitPaginatorAsync();
            StateHasChanged();

            await _localStorage.SetItemAsync<List<DisplayDoctor>>("doctors", OriginalDoctors);
            await _localStorage.SetItemAsync<List<DisplayPatient>>("patients", OriginalPatients);
            await _localStorage.SetItemAsync<List<DisplayTechnician>>("technicians", OriginalTechnicians);

        }











    }
     
    //Init table
    public async Task InitApplyAllFilters()
    {
        string[] nameWords = PatientNameFilter.ToLower().Trim().Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        FilteredPatients = OriginalPatients.Where
        (pat =>


            (nameWords.All(word => pat.FirstName.ToLower().Trim().Contains(word) ||
                                    pat.LastName.ToLower().Trim().Contains(word)))     // Name Filter -> any or 'words' in FirstName or LastName

            &&
            (
                // FilteredDoctors.Any(doc => doc.FirstName!.Trim().ToLower() == pat.DoctorFirstName.ToLower().Trim() &&   // If we Filtered by Doctors
                //                             doc.LastName!.Trim().ToLower() == pat.DoctorLastName.ToLower().Trim()

                FilteredDoctors.Any(doc => doc.Email!.ToLower().Trim() == pat.DoctorEmail.ToLower().Trim() // If we Filtered by Doctors
             )
             &&
             (


                // FilteredTechnicians.Any(tech => tech.FirstName!.Trim().ToLower() == pat.TechnicianFirstName.ToLower().Trim() &&   // If we Filtered by Technicians
                //                             tech.LastName!.Trim().ToLower() == pat.TechnicianLastName.ToLower().Trim()


                FilteredTechnicians.Any(tech => tech.Email!.ToLower().Trim() == pat.TechnicianEmail.ToLower().Trim() // If we Filtered by Technicians





             )
             )


            )
            &&
            (pat.DateAdded >= FromDateFilter && pat.DateAdded <= ToDateFilter)  // Date Range

         ).ToList();


    }
    public async Task InitApplySortings()
    {
        switch (PatientSortingMode)
        {
            case SortingMode.Ascending:
                FilteredPatients = FilteredPatients.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
                break;
            case SortingMode.Descending:
                FilteredPatients = FilteredPatients.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName).ToList();
                break;
            default:
                break;
        }
        switch (TechnicianSortingMode)
        {
            case SortingMode.Ascending:
                FilteredPatients = FilteredPatients.OrderBy(x => x.TechnicianFirstName).ThenBy(x => x.TechnicianLastName).ToList();
                break;
            case SortingMode.Descending:
                FilteredPatients = FilteredPatients.OrderByDescending(x => x.TechnicianFirstName).ThenByDescending(x => x.TechnicianLastName).ToList();
                break;
            default:
                break;
        }
        switch (DoctorSortingMode)
        {
            case SortingMode.Ascending:
                FilteredPatients = FilteredPatients.OrderBy(x => x.DoctorFirstName).ThenBy(x => x.DoctorLastName).ToList();
                break;
            case SortingMode.Descending:
                FilteredPatients = FilteredPatients.OrderByDescending(x => x.DoctorFirstName).ThenByDescending(x => x.DoctorLastName).ToList();
                break;
            default:
                break;
        }
        switch (DateSortingMode)
        {
            case SortingMode.Ascending:
                FilteredPatients = FilteredPatients.OrderBy(x => x.DateAdded).ToList();
                break;
            case SortingMode.Descending:
                FilteredPatients = FilteredPatients.OrderByDescending(x => x.DateAdded).ToList();
                break;
            default:
                break;
        }
    }


    private async void ClearDoctorsFilter()
    {

        FilteredDoctors = OriginalDoctors;
        await _localStorage.SetItemAsync<List<DisplayDoctor>>("filteredDoctors", FilteredDoctors);

        DisplayDoctorTextFilters = $"Select Doctors";
        ApplyAllFilters();
    }
    private async void ClearTechnicianFilter()
    {
        FilteredTechnicians = OriginalTechnicians;
        await _localStorage.SetItemAsync<List<DisplayTechnician>>("filteredTechnicians", FilteredTechnicians);

        DisplayTechnicianTextFilters = $"Select Technicians";
        ApplyAllFilters();
    }

    private async void ClearDateFilter()
    {
        FromDateFilter = DateTime.MinValue;
        ToDateFilter = DateTime.MaxValue;

        await _localStorage.SetItemAsync<DateTime>("fromDateFilter", FromDateFilter);
        await _localStorage.SetItemAsync<DateTime>("toDateFilter", ToDateFilter);


        DisplayDateFilter = (MarkupString)$"Select Period";
        ApplyAllFilters();
    }
    
    //Sorting Input
    private void SortByName()
    {
        if (PatientSortingMode == SortingMode.Descending || PatientSortingMode == SortingMode.None)
        {
            PatientSortingMode = SortingMode.Ascending;

            FilteredPatients = FilteredPatients.OrderBy(x => x.FirstName).ThenBy(x => x.LastName).ToList();
        }
        else if (PatientSortingMode == SortingMode.Ascending)
        {
            PatientSortingMode = SortingMode.Descending;

            FilteredPatients = FilteredPatients.OrderByDescending(x => x.FirstName).ThenByDescending(x => x.LastName).ToList();
        }

        _localStorage.SetItemAsync<string>("nameSortingMode", PatientSortingMode);
        paginator_CurrentPage = 1;
        PaginatorChanged();
    }
    private void SortByDate()
    {
        if (DateSortingMode == SortingMode.Descending || DateSortingMode == SortingMode.None)
        {
            DateSortingMode = SortingMode.Ascending;
            FilteredPatients = FilteredPatients.OrderBy(x => x.DateAdded).ToList();
        }
        else if (DateSortingMode == SortingMode.Ascending)
        {
            DateSortingMode = SortingMode.Descending;
            FilteredPatients = FilteredPatients.OrderByDescending(x => x.DateAdded).ToList();
        }

        _localStorage.SetItemAsync<string>("dateSortingMode", DateSortingMode);
        paginator_CurrentPage = 1;
        PaginatorChanged();
    }
    private void SortByDoctor()
    {
        if (DoctorSortingMode == SortingMode.Descending || DoctorSortingMode == SortingMode.None)
        {
            DoctorSortingMode = SortingMode.Ascending;
            FilteredPatients = FilteredPatients.OrderBy(x => x.DoctorFirstName).ThenBy(x => x.DoctorLastName).ToList();
        }
        else if (DoctorSortingMode == SortingMode.Ascending)
        {
            DoctorSortingMode = SortingMode.Descending;
            FilteredPatients = FilteredPatients.OrderByDescending(x => x.DoctorFirstName).ThenByDescending(x => x.DoctorLastName).ToList();
        }
        _localStorage.SetItemAsync<string>("doctorSortingMode", DoctorSortingMode);
        paginator_CurrentPage = 1;
        PaginatorChanged();
    }

    private void SortByTechnician()
    {
        if (TechnicianSortingMode == SortingMode.Descending || TechnicianSortingMode == SortingMode.None)
        {
            TechnicianSortingMode = SortingMode.Ascending;
            FilteredPatients = FilteredPatients.OrderBy(x => x.TechnicianFirstName).ThenBy(x => x.TechnicianLastName).ToList();
        }
        else if (TechnicianSortingMode == SortingMode.Ascending)
        {
            TechnicianSortingMode = SortingMode.Descending;
            FilteredPatients = FilteredPatients.OrderByDescending(x => x.TechnicianFirstName).ThenByDescending(x => x.TechnicianLastName).ToList();
        }
        _localStorage.SetItemAsync<string>("technicianSortingMode", TechnicianSortingMode);
        paginator_CurrentPage = 1;
        PaginatorChanged();
    }

    //Apply All Filters
    private void ApplyAllFilters()
    {
        string[] nameWords = PatientNameFilter.ToLower().Trim().Split(new char[] { ' ', '\t', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        FilteredPatients = OriginalPatients.Where
        (pat =>


            (nameWords.All(word => pat.FirstName.ToLower().Trim().Contains(word) ||
                                    pat.LastName.ToLower().Trim().Contains(word)))     // Name Filter -> any or 'words' in FirstName or LastName

            &&
            (
                // FilteredDoctors.Any(doc => doc.FirstName!.Trim().ToLower() == pat.DoctorFirstName.ToLower().Trim() &&   // If we Filtered by Doctors
                //                             doc.LastName!.Trim().ToLower() == pat.DoctorLastName.ToLower().Trim()

                FilteredDoctors.Any(doc => doc.Email!.ToLower().Trim() == pat.DoctorEmail.ToLower().Trim() // If we Filtered by Doctors
             )
             &&
             (


                // FilteredTechnicians.Any(tech => tech.FirstName!.Trim().ToLower() == pat.TechnicianFirstName.ToLower().Trim() &&   // If we Filtered by Technicians
                //                             tech.LastName!.Trim().ToLower() == pat.TechnicianLastName.ToLower().Trim()


                FilteredTechnicians.Any(tech => tech.Email!.ToLower().Trim() == pat.TechnicianEmail.ToLower().Trim() // If we Filtered by Technicians

 
             )
             )


            )
            &&
            (pat.DateAdded >= FromDateFilter && pat.DateAdded <= ToDateFilter)  // Date Range 

         ).ToList();

        DateSortingMode = SortingMode.Ascending;
        SortByDate();
    }


    //Filtering
    private void HandleNameFiltered(ChangeEventArgs args)
    {

        try
        {
            PatientNameFilter = args.Value!.ToString()!;
            _localStorage.SetItemAsync<string>("patientNameFilter", PatientNameFilter);
            ApplyAllFilters();

        }
        catch (Exception)
        {

        }
    }










}
