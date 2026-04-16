using Voluntrio.Organizations;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "Voluntrio.Api")
            .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName));

    // Add services to the container.
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();
    builder.Services.AddOrganizationsModule(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();

    app.MapOrganizationsModuleEndpoints();

    app.MapGet("/", () =>
    {
        return "Welcome to the Voluntrio API!";
    })
    .WithName("HelloWorld");

    app.Run();
}
catch (Exception exception)
{
    Log.Fatal(exception, "Voluntrio API terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}


