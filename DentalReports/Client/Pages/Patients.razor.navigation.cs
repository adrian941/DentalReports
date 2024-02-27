using Microsoft.JSInterop;

namespace DentalReports.Client.Pages;

public partial class Patients
{
    private async Task ReloadPage()
    {
        await _jsRuntime.InvokeVoidAsync("location.reload", true);
    }
    public void ShowVideo(int PatientId)
    {
        _navigationManager.NavigateTo($"/video/{PatientId}");
    }
    public void ShowStl(int PatientId)
    {
        _navigationManager.NavigateTo($"/3d-viewer/{PatientId}");
    }

    public void ShowPdf(int PatientId)
    {
        _navigationManager.NavigateTo($"/pdf/{PatientId}");
    }
}
