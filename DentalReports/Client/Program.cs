using Blazored.LocalStorage;
using Blazored.Modal;
using DentalReports.Client;
using DentalReports.Shared.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Toolbelt.Blazor.Extensions.DependencyInjection;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("DentalReports.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
    .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Supply HttpClient instances that include access tokens when making requests to the server project
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("DentalReports.ServerAPI"));

builder.Services.AddApiAuthorization();
builder.Services.AddBlazoredModal();

builder.Services.AddScoped<IDoctorService,DoctorService>();
builder.Services.AddPWAUpdater();
builder.Services.AddBlazoredLocalStorage(); 





await builder.Build().RunAsync();
