using AzureResourcesInfo.Api;
using AzureResourcesInfo.Api.Services;
using Scalar.AspNetCore;
using System.Net.Http.Headers;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: false);

builder.Services.AddSingleton<IAzureResourcesReader, AzureClientsResourcesReader>();

builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient("AzureClient",
    client =>
    {
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .AddStandardResilienceHandler();

WebApplication app = builder.Build();

app.UseHttpsRedirection();
app.UseCors();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

RouteGroupBuilder azureGroup = app.MapGroup("/api/azure")
    .WithTags("Azure");
azureGroup.MapGet("cosmosDbAccounts", EndpointHandlers.GetCosmosDbAccounts)
    .WithName(nameof(EndpointHandlers.GetCosmosDbAccounts));

app.Run();
