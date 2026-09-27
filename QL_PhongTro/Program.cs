using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro;
using QL_PhongTro.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
builder.Services.AddSingleton<QL_PhongTro.Services.GiayToImageStore>();
builder.Services.AddScoped<QL_PhongTro.Services.HoSoAccess>();

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
var configuredDatabasePath = builder.Configuration["DatabasePath"];
var databasePath = string.IsNullOrWhiteSpace(configuredDatabasePath)
    ? Path.Combine(dataDir, "local-dev.sqlite")
    : Path.GetFullPath(Path.IsPathRooted(configuredDatabasePath)
        ? configuredDatabasePath
        : Path.Combine(builder.Environment.ContentRootPath, configuredDatabasePath));
if (!File.Exists(databasePath))
    throw new FileNotFoundException("Existing local SQLite database was not found; refusing to create a new database.", databasePath);

var connectionString = $"Data Source={databasePath}";
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Register";
        options.AccessDeniedPath = "/";
    });
builder.Services.AddSingleton<RegistrationSettings>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();
app.Run();
