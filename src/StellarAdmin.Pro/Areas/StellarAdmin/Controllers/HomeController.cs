using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.Pro.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
