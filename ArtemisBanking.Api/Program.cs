using System.Text;
using System.Text.Json.Serialization;
using ArtemisBanking.Api.Extensions;
using ArtemisBanking.Domain.Settings;
using ArtemisBanking.Infrastructure.Shared;
using ArtemisBanking.Infraestructure.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ArtemisBanking.Application.Mappings.EntitiesAndDtos;

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
    typeof(ArtemisBanking.Application.Mappings.DtosAndViewModels.AdminUserProfile).Assembly,
    typeof(SavingsAccountProfile).Assembly);

builder.Services.AddAuthorization();

var app = builder.Build();

// Configuración de Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwaggerExtension();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Asegurarse de que todos los servicios estén configurados antes de sembrar datos
await app.SeedIdentityAsync();

app.UseHealthChecks("/health");

app.MapControllers();

await app.RunAsync();
