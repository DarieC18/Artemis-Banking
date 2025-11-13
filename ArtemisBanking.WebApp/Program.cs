using ArtemisBanking.Infrastructure.Shared;
using ArtemisBanking.Infraestructure.Identity;
using ArtemisBanking.Infraestructure.Identity.Mappings;
using ArtemisBanking.WebApp.Mappings;
using Microsoft.AspNetCore.Builder;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddAutoMapper(typeof(AppUserMappingProfile).Assembly, typeof(AccountWebMappingProfile).Assembly);
builder.Services.AddSingleton(TimeProvider.System);

var app = builder.Build();

await app.SeedIdentityAsync();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();


