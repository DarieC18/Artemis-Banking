using ArtemisBanking.Application;
using ArtemisBanking.Infraestructure.Identity;
using ArtemisBanking.Infraestructure.Identity.Mappings;
using ArtemisBanking.Infrastructure.Persistence;
using ArtemisBanking.Infrastructure.Shared;
using ArtemisBanking.WebApp.Mappings;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSharedInfrastructure(builder.Configuration);
builder.Services.AddIdentityInfrastructure(builder.Configuration);
builder.Services.AddAutoMapper(typeof(AppUserMappingProfile).Assembly, typeof(AccountWebMappingProfile).Assembly);
builder.Services.AddSingleton(TimeProvider.System);

//IoC
builder.Services.AddApplicationLayerIoc(builder.Configuration);
builder.Services.AddPersistenceLayerIoc(builder.Configuration);

var app = builder.Build();

await app.SeedIdentityAsync();
//await app.SeedBankingDataAsync(); Seeder para TESTING

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


