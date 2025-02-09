var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.AddAzureBlobClient("blobs"); 

builder.ConfigureFunctionsWebApplication();

await builder.Build().RunAsync();
