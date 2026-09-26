namespace QL_PhongTro.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open QL_PhongTro.Models

open QL_PhongTro

type HomeController (logger : ILogger<HomeController>, settings: RegistrationSettings) =
    inherit Controller()

    member this.Index () =
        this.ViewData.["RegistrationEnabled"] <- box settings.EnableDuplicateCheck
        this.View()

    member this.Privacy () =
        this.View()

    [<HttpPost>]
    member this.ToggleRegistration() =
        settings.EnableDuplicateCheck <- not settings.EnableDuplicateCheck
        this.TempData.["Msg"] <- (if settings.EnableDuplicateCheck then "Duplicate check enabled" else "Duplicate check disabled")
        this.RedirectToAction("Index") :> IActionResult

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })
