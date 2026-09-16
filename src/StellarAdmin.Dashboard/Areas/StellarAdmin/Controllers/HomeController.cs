using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.Dashboard.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
