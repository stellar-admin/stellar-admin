using Microsoft.AspNetCore.Mvc;

namespace ShellPlayground.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
