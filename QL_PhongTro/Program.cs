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

var resetStagingDemo = args.Contains("--reset-staging-demo");
if (builder.Environment.IsStaging() && string.IsNullOrWhiteSpace(configuredDatabasePath))
    throw new InvalidOperationException("Staging requires an explicit DatabasePath; the local development database is never a staging default.");
if (resetStagingDemo)
{
    if (!builder.Environment.IsStaging())
        throw new InvalidOperationException("--reset-staging-demo is available only in the Staging environment.");
    if (!bool.TryParse(builder.Configuration["Staging:AllowReset"], out var allowReset) || !allowReset)
        throw new InvalidOperationException("Set Staging:AllowReset=true explicitly before resetting staging data.");
    if (string.Equals(Path.GetFullPath(databasePath), Path.GetFullPath(Path.Combine(dataDir, "local-dev.sqlite")), StringComparison.OrdinalIgnoreCase))
        throw new InvalidOperationException("Refusing to reset the local development database.");
    var demoPassword = builder.Configuration["Staging:DemoPassword"];
    if (string.IsNullOrWhiteSpace(demoPassword))
        throw new InvalidOperationException("Set Staging:DemoPassword through an environment secret before seeding.");
    await StagingDemoSeeder.ResetAsync(databasePath, Path.Combine(dataDir, "permissions.seed.json"), demoPassword);
    return;
}

var initFlags = new[] { "--create-permission-demo", "--initialize-permissions", "--initialize-password-security", "--initialize-services", "--initialize-service-invoices" };
bool isInit = args.Any(a => initFlags.Contains(a));

// If database doesn't exist, create it via EF EnsureCreated (for init commands or dev)
if (!File.Exists(databasePath))
{
    if (isInit || builder.Environment.IsDevelopment())
    {
        var connStr = new Microsoft.Data.Sqlite.SqliteConnectionStringBuilder
        {
            DataSource = databasePath, Mode = Microsoft.Data.Sqlite.SqliteOpenMode.ReadWriteCreate, ForeignKeys = true
        }.ToString();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connStr).Options;
        using var db = new AppDbContext(options);
        db.Database.EnsureCreated();
        // Also run auth schema init to add missing columns
        AuthSchemaInitializer.Initialize(databasePath);
    }
    else
    {
        throw new FileNotFoundException("Existing local SQLite database was not found; refusing to create a new database.", databasePath);
    }
}

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
if (args.Contains("--initialize-password-security"))
{
    PasswordSchemaInitializer.Initialize(databasePath);
    return;
}
if (args.Contains("--initialize-services"))
{
    DichVuSchemaInitializer.Initialize(databasePath);
    return;
}
if (args.Contains("--initialize-service-invoices"))
{
    DichVuSchemaInitializer.InitializeInvoices(databasePath);
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

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<ITimeProvider, SystemTimeProvider>();
builder.Services.AddMemoryCache();
builder.Services.AddScoped<TokenBlacklistService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AuthService>();
builder.Services.Configure<PasswordResetOptions>(builder.Configuration.GetSection("PasswordReset"));
builder.Services.AddScoped<IPasswordEmailSender, PasswordEmailSender>();
builder.Services.AddScoped<PasswordResetService>();
builder.Services.AddScoped<SessionVersionStore>();
builder.Services.AddScoped<DichVuService>();
builder.Services.AddScoped<HoaDonDichVuService>();
builder.Services.Configure<DichVuMacDinhOptions>(builder.Configuration.GetSection("DichVuMacDinh"));

builder.Services.AddAuthentication()
.AddJwtBearer(options =>
{
    options.Events = new JwtBearerEvents
    {
        OnTokenValidated = context =>
        {
            var id = context.Principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var version = context.Principal?.FindFirst(SessionVersionStore.ClaimType)?.Value;
            if (!int.TryParse(id, out var accountId) || !context.HttpContext.RequestServices.GetRequiredService<SessionVersionStore>().IsValid(accountId, version))
                context.Fail("Phiên đăng nhập đã bị vô hiệu hóa.");
            return Task.CompletedTask;
        }
    };
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
    AuthSchemaInitializer.Initialize(databasePath);
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    RoomSchemaInitializer.EnsureSchema(db);
}


app.MapRazorPages();
app.Run();

public partial class Program { }
