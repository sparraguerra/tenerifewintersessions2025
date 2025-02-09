var builder = DistributedApplication.CreateBuilder(args);

var storage = builder.AddAzureStorage("storage").RunAsEmulator(azurite =>
            {
                azurite.WithContainerName("storage");
                azurite.WithBlobPort(59012).WithQueuePort(59013).WithTablePort(59014);
            })
            .ConfigureInfrastructure((infrastructure) =>
            {
                var storageAccount = infrastructure.GetProvisionableResources().OfType<StorageAccount>().FirstOrDefault(r => r.BicepIdentifier == "storage")
                    ?? throw new InvalidOperationException($"Could not find configured storage account with name 'storage'");

                // Storage Account Contributor and Storage Blob Data Owner roles are required by the Azure Functions host
                var principalTypeParameter = new ProvisioningParameter(AzureBicepResource.KnownParameters.PrincipalType, typeof(string));
                var principalIdParameter = new ProvisioningParameter(AzureBicepResource.KnownParameters.PrincipalId, typeof(string));
                infrastructure.Add(storageAccount.CreateRoleAssignment(StorageBuiltInRole.StorageAccountContributor, principalTypeParameter, principalIdParameter));
                infrastructure.Add(storageAccount.CreateRoleAssignment(StorageBuiltInRole.StorageBlobDataOwner, principalTypeParameter, principalIdParameter));

                // Ensure that public access to blobs is disabled
                storageAccount.AllowBlobPublicAccess = false;
            });

var blobs = storage.AddBlobs("blobs");

var daprComponents = new DaprComponentOptions()
{
    LocalPath = Path.Combine("..", "dapr"),
};

var stateStore = builder.AddDaprStateStore("statestore", daprComponents);
var pubSub = builder.AddDaprPubSub("pubsub", daprComponents);

builder.AddAzureFunctionsProject<Projects.AspireNetFunctions_Functions>("functionapp")
                        .WithDaprSidecar(new DaprSidecarOptions()
                        {
                            AppId = "functionapp",
                            ResourcesPaths = [Path.Combine("..", "dapr")] 
                        })
                       .WithReference(blobs) 
                       .WithReference(stateStore)
                       .WithReference(pubSub)
                       .WaitFor(blobs)
                       .WithHostStorage(storage)
                       .WithExternalHttpEndpoints()
                       .WithHttpHealthCheck("/");

builder.AddProject<Projects.AspireNetFunctions_Api>("api")
                        .WithDaprSidecar(new DaprSidecarOptions()
                        {
                            AppId = "api",
                            ResourcesPaths = [Path.Combine("..", "dapr")]
                        }) 
                        .WithReference(stateStore)
                        .WithReference(pubSub)
                        .WithExternalHttpEndpoints()
                        .WithHttpHealthCheck("/health");

await builder.Build().RunAsync();
