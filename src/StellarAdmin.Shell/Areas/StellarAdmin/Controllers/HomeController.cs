using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.Shell.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
