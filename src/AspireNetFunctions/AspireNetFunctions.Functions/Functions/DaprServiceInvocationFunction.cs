namespace AspireNetFunctions.Functions.Functions;

public class DaprServiceInvocationFunction(DaprClient daprClient, ILogger<DaprServiceInvocationFunction> logger)
{
    private readonly DaprClient _daprClient = daprClient;
    private readonly ILogger<DaprServiceInvocationFunction> _logger = logger;

    [Function(nameof(DaprServiceInvocationFunction))]
    public async Task<IActionResult> RunDaprServiceInvocationFunction([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "external")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var response =  await _daprClient.InvokeMethodAsync<IEnumerable<WeatherForecastResult>>(HttpMethod.Get, "api", "weatherforecast", CancellationToken.None);

        _logger.LogInformation("Response: {Response}", response);

        return new OkObjectResult(response);
    }
}
