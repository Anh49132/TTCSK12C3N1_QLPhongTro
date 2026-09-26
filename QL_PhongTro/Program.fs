namespace QL_PhongTro

#nowarn "20"

open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Threading.Tasks
open Microsoft.AspNetCore
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.HttpsPolicy
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.EntityFrameworkCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Authentication
open QL_PhongTro
open QL_PhongTro.Data
open System.Security.Claims
open System.IO

module Program =
    let exitCode = 0

    [<EntryPoint>]
    let main args =
        let builder = WebApplication.CreateBuilder(args)

        builder
            .Services
            .AddControllersWithViews()
            .AddRazorRuntimeCompilation()

        builder.Services.AddRazorPages()

        // Configure SQLite DbContext for local development (data/local-dev.sqlite)
        let dataDir = Path.Combine(builder.Environment.ContentRootPath, "data")
        if not (Directory.Exists dataDir) then Directory.CreateDirectory(dataDir) |> ignore
        let connString = sprintf "Data Source=%s" (Path.Combine(dataDir, "local-dev.sqlite"))
        builder.Services.AddDbContext<AppDbContext>(fun options -> options.UseSqlite(connString) |> ignore) |> ignore

        // Cookie authentication (lightweight)
        builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
            .AddCookie(fun options ->
                options.LoginPath <- "/Account/Register"
                options.AccessDeniedPath <- "/"
            ) |> ignore

        // Register singleton settings for runtime control of features
        builder.Services.AddSingleton<RegistrationSettings>(RegistrationSettings()) |> ignore

        let app = builder.Build()

        if not (builder.Environment.IsDevelopment()) then
            app.UseExceptionHandler("/Home/Error")
            app.UseHsts() |> ignore // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.

        app.UseHttpsRedirection()

        app.UseStaticFiles()
        app.UseRouting()
        app.UseAuthentication()
        app.UseAuthorization()

        app.MapControllerRoute(name = "default", pattern = "{controller=Home}/{action=Index}/{id?}")

        // Ensure local dev database and tables are created
        use scope = app.Services.CreateScope()
        let db = scope.ServiceProvider.GetRequiredService<AppDbContext>()
        db.Database.EnsureCreated() |> ignore

        app.MapRazorPages()

        app.Run()

        exitCode
