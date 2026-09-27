using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using QL_PhongTro;
using QL_PhongTro.Data;
using QL_PhongTro.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
var configuredDatabasePath = builder.Configuration["DatabasePath"];
var databasePath = string.IsNullOrWhiteSpace(configuredDatabasePath)
    ? Path.Combine(dataDir, "local-dev.sqlite")
    : Path.GetFullPath(Path.IsPathRooted(configuredDatabasePath)
        ? configuredDatabasePath
        : Path.Combine(builder.Environment.ContentRootPath, configuredDatabasePath));
if (!File.Exists(databasePath))
    throw new FileNotFoundException("Existing local SQLite database was not found; refusing to create a new database.", databasePath);

var demoIndex = Array.IndexOf(args, "--create-permission-demo");
if (demoIndex >= 0)
{
    if (demoIndex + 1 >= args.Length) throw new ArgumentException("Provide a new SQLite path after --create-permission-demo.");
    PermissionDemo.Create(databasePath, args[demoIndex + 1], Path.Combine(dataDir, "permissions.seed.json"));
    return;
}
if (args.Contains("--initialize-permissions"))
{
    PermissionSchemaInitializer.Initialize(databasePath, Path.Combine(dataDir, "permissions.seed.json"));
    return;
}
var connectionString = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
{
    DataSource = databasePath, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWrite, ForeignKeys = true
}.ToString();
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

// Isolate demo/real databases even when both run on localhost with the same key ring.
var authScope = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Path.GetFullPath(databasePath).ToUpperInvariant())))[..24];
builder.Services.AddDataProtection().SetApplicationName("QLPhongTro:" + authScope);
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".QLPhongTro." + authScope;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.EventsType = typeof(AppCookieEvents);
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
    });
builder.Services.AddSingleton<RegistrationSettings>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<AppCookieEvents>();

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

public partial class Program { }
