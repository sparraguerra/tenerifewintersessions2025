namespace AspireNetFunctions.DaprFunctions.Functions;


public class DaprTopicTriggerFunction
{
    /// <summary>
    /// Visit https://aka.ms/azure-functions-dapr to learn how to use the Dapr extension.
    /// These tasks should be completed prior to running :
    ///   1. Install Dapr
    /// Start function app with Dapr: dapr run --app-id functionapp --app-port 3001 --dapr-http-port 3501 -- func host start
    /// Function will be invoked by Timer trigger and publish messages to message bus.
    /// </summary>
    /// <param name="functionContext">Function context.</param>
    [Function(nameof(TimerTriggerFunction))]
    [DaprPublishOutput(PubSubName = "pubsub", Topic = "A")]
    public DaprPubSubEvent TimerTriggerFunction([TimerTrigger("*/10 * * * * *")] object myTimer, FunctionContext functionContext)
    {
        var log = functionContext.GetLogger(nameof(TimerTriggerFunction));
        log.LogInformation("C# DaprPublish output binding function processed a request.");

        return new DaprPubSubEvent("Invoked by Timer trigger: " + $"Hello, World! The time is {System.DateTime.Now}");
    }

    // Below Azure function will receive message published on topic A, and it will log the message
    /// <summary>
    /// Visit https://aka.ms/azure-functions-dapr to learn how to use the Dapr extension.
    /// This function will get invoked when a message is published on topic A
    /// </summary>
    /// <param name="subEvent">Cloud event sent by Dapr runtime.</param>
    /// <param name="functionContext">Function context.</param>
    [Function(nameof(DaprTopicTriggerFunction))]
    public static void RunDaprTopicTriggerFunction([DaprTopicTrigger("pubsub", Topic = "A")] CloudEvent subEvent, FunctionContext functionContext)
    {
        var log = functionContext.GetLogger(nameof(DaprTopicTriggerFunction));
        log.LogInformation("C# Dapr Topic Trigger function processed a request from the Dapr Runtime.");
        log.LogInformation("Topic A received a message: {Data}", subEvent.Data);
    }
} 