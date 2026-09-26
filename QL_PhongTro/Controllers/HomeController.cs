using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QL_PhongTro.Models;

namespace QL_PhongTro.Controllers;

public class HomeController(RegistrationSettings settings) : Controller
{
    public IActionResult Index()
    {
        ViewData["RegistrationEnabled"] = settings.EnableDuplicateCheck;
        return View();
    }

    public IActionResult Privacy() => View();

    [HttpPost]
    public IActionResult ToggleRegistration()
    {
        settings.EnableDuplicateCheck = !settings.EnableDuplicateCheck;
        TempData["Msg"] = settings.EnableDuplicateCheck ? "Duplicate check enabled" : "Duplicate check disabled";
        return RedirectToAction("Index");
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
    });
}
