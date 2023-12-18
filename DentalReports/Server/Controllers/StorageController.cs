using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;

namespace DentalReports.Server.Controllers;

[Route("api/[controller]/[action]")]
[ApiController]
public class StorageController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private string blobStorageName = string.Empty;
    private string containerName = string.Empty;
 



    public StorageController(IConfiguration configuration)
    {
        _configuration = configuration;
        blobStorageName = _configuration.GetValue<string>("AzureBlobStorageName")!;
        containerName = _configuration.GetValue<string>("AzureContainerName")!;
     
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("/api/Storage/getFileLink/{FileName}")]
    public string getFileLink(string FileName)
    {

        string videoSource = $"https://{blobStorageName}.blob.core.windows.net/{containerName}/{FileName}";

        return videoSource;

    }

    [AllowAnonymous]
    [HttpPost]
    public async Task<ActionResult> UploadFiles(List<IFormFile> files)
    {



        foreach (var file in files)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            // Set the position of the stream to 0
            memoryStream.Position = 0;

            string blobName = file.FileName;
            var blobClient = new BlobClient(_configuration.GetConnectionString("AzureStorage"), containerName, blobName);

            await blobClient.UploadAsync(memoryStream);
        }



        return Ok("hehe!");

    }




}
