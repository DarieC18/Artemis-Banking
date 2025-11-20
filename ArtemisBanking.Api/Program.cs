using ArtemisBanking.Api.Extensions;
using ArtemisBanking.Application;
using ArtemisBanking.Infraestructure.Identity;
using ArtemisBanking.Infrastructure.Persistence;
using ArtemisBanking.Infrastructure.Shared;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(opt =>
{
    opt.Filters.Add(new ProducesAttribute("application/json"));
}).ConfigureApiBehaviorOptions(opt =>
{
    opt.SuppressInferBindingSourcesForParameters = true;
    opt.SuppressMapClientErrors = true;
}).AddJsonOptions(opt =>
{
    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecks();
builder.Services.AddApiVersioningExtension();
builder.Services.AddSwaggerExtension();
builder.Services.AddHealthChecks();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddIdentityInfrastructureForApi(builder.Configuration);
builder.Services.AddAutoMapper(
    typeof(ArtemisBanking.Infraestructure.Identity.Mappings.AppUserMappingProfile).Assembly,
    typeof(ArtemisBanking.Application.Mappings.DtosAndViewModels.AccountMappingProfile).Assembly);

builder.Services.AddApplicationLayerIoc(builder.Configuration);
builder.Services.AddPersistenceLayerIoc(builder.Configuration);
builder.Services.AddAuthorization();

var app = builder.Build();

await app.SeedIdentityAsync();

// Configuración de Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseHealthChecks("/health");

app.MapControllers();

await app.RunAsync();
