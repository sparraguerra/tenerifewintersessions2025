var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddHttpContextAccessor();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.MapSwaggerEndpoints();

app.UseCloudEvents();
app.MapSubscribeHandler();
app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", (ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("weatherforecast");
    logger.LogInformation("Processing request to /weatherforecast");

    var forecast = Enumerable.Range(1, 5).Select(index =>
                        new WeatherForecastResult
                        (
                            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                            Random.Shared.Next(-20, 55),
                            summaries[Random.Shared.Next(summaries.Length)]
                        ))
                        .ToArray();

    logger.LogInformation("Request to /weatherforecast processed");
    return forecast;
})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.MapPost("/upload-result", ([FromBody] UploadResult result, ILoggerFactory loggerFactory) =>
{
    var logger = loggerFactory.CreateLogger("upload-result");
    logger.LogInformation("Processing topic subscription message"); 
    logger.LogInformation("Message body: {Body}", result);

    var jsonMessage = JsonSerializer.Serialize(new ProcessQueueResult(true), ProcessQueueResultSerializationContext.Default.ProcessQueueResult);

    return Task.FromResult(jsonMessage);

})
.WithName("UploadResult")
.WithOpenApi()
.WithTopic("pubsub", "upload-result");

await app.RunAsync();


