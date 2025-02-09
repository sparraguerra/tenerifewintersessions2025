namespace AspireNetFunctions.Api;

public static class Extensions
{
    public static WebApplication MapSwaggerEndpoints(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        return app;
    }
}
