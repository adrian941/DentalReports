using System.Net.Http.Json;

namespace DentalReports.Client.Pages;

public partial class Patients
{
    public async Task CalculateStorageDetailsOfTechnician()
    {


        //fetch both details from API
        totalPatientsSizeGB = (await _httpClient.GetFromJsonAsync<double>("api/technician/getTotalPatientsSizeGB"));
        sizeStoragePlanGB = (await _httpClient.GetFromJsonAsync<double>("api/technician/getStoragePlanGB"));

        //save both details to LocalStorage
        await _localStorage.SetItemAsync<double>("totalPatientsSizeGB", totalPatientsSizeGB);
        await _localStorage.SetItemAsync<double>("sizeStoragePlanGB", sizeStoragePlanGB);

    }
}
