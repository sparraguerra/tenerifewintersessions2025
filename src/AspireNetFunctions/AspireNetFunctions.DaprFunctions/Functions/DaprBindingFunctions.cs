namespace AspireNetFunctions.DaprFunctions.Functions;

public static class OrderService
{ 
    /// <summary>
    /// Sample to use a Dapr Invoke Output Binding to perform a Dapr Server Invocation operation hosted in another Darp'd app.
    /// Here this function acts like a proxy
    /// </summary>
    [Function(nameof(OrderService))]
    [DaprInvokeOutput(AppId = "{appId}", MethodName = "{methodName}", HttpVerb = "post")]
    public static async Task<InvokeMethodParameters> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "invoke/{appId}/{methodName}")] HttpRequestData req,
            FunctionContext functionContext)
    {
        var log = functionContext.GetLogger(nameof(OrderService));
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        //print the received payload
        log.LogInformation("Received Payload OrderService: {RequestBody}", requestBody);

        var outputContent = new InvokeMethodParameters
        {
            Body = requestBody
        };

        return outputContent;
    }
}

public static class CreateNewOrder
{
    /// <summary>
    /// Example to use Dapr Service Invocation Trigger and Dapr State Output binding to persist a new state into statestore
    /// </summary>
    [Function(nameof(CreateNewOrder))]
    [DaprStateOutput("statestore", Key = "order")]
    public static JsonElement Run(
        [DaprServiceInvocationTrigger] JsonElement payload,
        FunctionContext functionContext)
    {
        var log = functionContext.GetLogger(nameof(CreateNewOrder));
        log.LogInformation("C# function processed a CreateNewOrder request from the Dapr Runtime.");

        //print the received payload
        log.LogInformation("Received Payload CreateNewOrder: {Payload}", JsonSerializer.Serialize(payload));

        // payload must be of the format { "data": { "value": "some value" } }
        payload.TryGetProperty("data", out JsonElement data);


        log.LogInformation("{Data}", data);
        return data;
    }
}

public static class RetrieveOrder
{
    /// <summary>
    /// Example to use Dapr Service Invocation Trigger and Dapr State input binding to retrieve a saved state from statestore
    /// </summary>
    [Function(nameof(RetrieveOrder))]
    public static JsonElement? Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req,
                           [DaprStateInput("%StateStoreName%", Key = "order")] JsonElement? data,
                           FunctionContext functionContext)
    {
        var log = functionContext.GetLogger(nameof(RetrieveOrder));
        log.LogInformation("C# function processed a request from the Dapr Runtime.");

        //print the fetched state value
        log.LogInformation("Retrieved order:{Payload}", JsonSerializer.Serialize(data));

        return data;
    }
}


public static class StateInputBinding
{
    [Function(nameof(StateInputBinding))]
    public static string Run(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "state/{key}")] HttpRequestData req,
        [DaprStateInput("%StateStoreName%", Key = "{key}")] string state,
        FunctionContext functionContext)
    {
        var log = functionContext.GetLogger(nameof(StateInputBinding));
        log.LogInformation("C# HTTP trigger function processed a request.");


        log.LogInformation("State retrieved {State}", state);

        return state;
    }
}

public static class StateOutputBinding
{
    [Function(nameof(StateOutputBinding))]
    [DaprStateOutput("%StateStoreName%", Key = "{key}")]
    public static async Task<string> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "state/{key}")] HttpRequestData req,
                                FunctionContext functionContext)
    {
        var log = functionContext.GetLogger(nameof(StateOutputBinding));
        log.LogInformation("C# HTTP trigger function processed a request.");

        string requestBody = await (new StreamReader(req.Body)).ReadToEndAsync();

        log.LogInformation("Received Payload: {RequestBody}", requestBody);

        return requestBody;
    }
}