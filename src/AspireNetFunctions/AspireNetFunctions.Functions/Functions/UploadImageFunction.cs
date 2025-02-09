namespace AspireNetFunctions.Functions.Functions;

public class UploadImageFunction(BlobServiceClient client, ILogger<UploadImageFunction> logger)
{
    private readonly BlobContainerClient _blobContainerClient = client.GetBlobContainerClient("uploads");
    private readonly ILogger<UploadImageFunction> _logger = logger;

    [Function(nameof(UploadImageFunction))]
    public async Task<IActionResult> RunUploadImageFunction(
                                        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload/{fileName}")] HttpRequest req, 
                                        string fileName)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");
        if (req.ContentType != System.Net.Mime.MediaTypeNames.Application.Octet)
        {
            _logger.LogError("Invalid content type {ContentType}", req.ContentType);
            return new BadRequestResult();
        }

        try
        {
            await _blobContainerClient.CreateIfNotExistsAsync(); 
            await _blobContainerClient.UploadBlobAsync(fileName, req.Body);
            _logger.LogInformation("Uploaded file {FileName}", fileName);

            return new CreatedResult();
        }   
        catch (Azure.RequestFailedException ex) when (ex.ErrorCode == "BlobAlreadyExists")
        {
            _logger.LogError(ex, "Failed to upload file {FileName} because it already exists.", fileName);
            return new ConflictResult();
        }
      
    }
}
