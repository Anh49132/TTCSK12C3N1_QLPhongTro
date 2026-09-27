using Microsoft.AspNetCore.Authentication.Cookies;

using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QL_PhongTro;
using QL_PhongTro.Data;

using QL_PhongTro.Authorization;

using QL_PhongTro.Services;
using System.Text;


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

var connectionString = $"Data Source={databasePath}";
    builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(connectionString));

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<RegistrationSettings>();
builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<TokenBlacklistService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.SecretKey)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddCookie(options =>
{
    options.LoginPath = "/Account/Register";
    options.AccessDeniedPath = "/";
});


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
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");



using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    RoomSchemaInitializer.EnsureSchema(db);
}


app.MapRazorPages();
app.Run();

public partial class Program { }
