namespace AspireNetFunctions.Functions.Functions;

public class ThumbnailGeneratorFunction(IDaprPubSubMessageService daprPubSubMessageService,
                                        ILogger<ThumbnailGeneratorFunction> logger)
{
    private readonly IDaprPubSubMessageService _daprPubSubMessageService = daprPubSubMessageService;
    private readonly ILogger<ThumbnailGeneratorFunction> _logger = logger;
    private const int TargetHeight = 128;

    [Function(nameof(ThumbnailGeneratorFunction))]
    [BlobOutput("thumbnails/resized-{name}", Connection = "blobs")]
    public async Task<byte[]> RunThumbnailGeneratorFunction([BlobTrigger("uploads/{name}", Connection = "blobs")] byte[] image,  string name)
    {
        try
        {
            using var resizedStream = GetResizedImageStream(name, image, SKEncodedImageFormat.Jpeg);
            await SendPubSubMessageAsync(name); 
            return resizedStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating thumbnail for image: {Name}. Exception: {Message}", name, ex.Message);
        }
        return [];
    }

    private MemoryStream GetResizedImageStream(string name, byte[] image, SKEncodedImageFormat format)
    { 
        using var originalBitmap = SKBitmap.Decode(image);

        if(originalBitmap is null)
        {
            _logger.LogInformation("originalBitmap is null");
            return new MemoryStream();
        }

        var scale = (float)TargetHeight / originalBitmap.Height;
        var targetWidth = (int)(originalBitmap.Width * scale);

        using var resizedBitmap = originalBitmap.Resize(new SKImageInfo(targetWidth, TargetHeight), SKSamplingOptions.Default);

        using var imageResized = SKImage.FromBitmap(resizedBitmap);

        // Do not put in a using, as this is returned to the caller.
        var resizedStream = new MemoryStream();

        imageResized.Encode(format, 100).SaveTo(resizedStream);

        _logger.LogInformation("Resized image {Name} from {OriginalWidth}x{OriginalHeight} to {Width}x{Height}.",
                              name, originalBitmap.Width, originalBitmap.Height, targetWidth, TargetHeight);

        return resizedStream;
    }
   
    private async Task SendPubSubMessageAsync(string name)
    {
        var message = new UploadResult(name, true);
        _logger.LogDebug("Signaling upload of {Name}", name);

        await _daprPubSubMessageService.PublishEventAsync(message, "upload-result", "pubsub", CancellationToken.None); 

        _logger.LogInformation("Signaled upload of {Name}", name);
    }
}