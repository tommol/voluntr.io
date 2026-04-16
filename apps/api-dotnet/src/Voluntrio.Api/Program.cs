using Voluntrio.Organizations;
using Serilog;
using Asp.Versioning;
using Voluntrio.Api.OpenApi;
using Microsoft.OpenApi;

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
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1);
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.ReportApiVersions = true;
    });
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo { Title = "Voluntrio API", Version = "v1" });
        options.OperationFilter<SubstituteApiVersionRouteParamFilter>();
        options.OperationFilter<OrganizationActorHeadersOperationFilter>();
    });
    builder.Services.AddOrganizationsModule(builder.Configuration);

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwagger(options =>
    {
        options.RouteTemplate = "openapi/{documentName}/openapi.json";
    });

    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "openapi";
        options.SwaggerEndpoint("/openapi/v1/openapi.json", "Voluntrio API V1");
    });

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


