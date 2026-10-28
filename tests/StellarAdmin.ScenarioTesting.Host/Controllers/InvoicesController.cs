using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.ScenarioTesting.Host.Controllers;

public sealed class InvoicesController : Controller
{
    public IActionResult Index() => Content("Invoices");
}
