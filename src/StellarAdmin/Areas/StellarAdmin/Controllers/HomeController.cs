using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.Areas.StellarAdmin.Controllers;

[Area("StellarAdmin")]
internal sealed class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
