using Microsoft.AspNetCore.Mvc;

namespace StellarAdmin.Dashboard.IntegrationTests.Fixtures;

public sealed class InvoicesController : Controller
{
    public IActionResult Index() => Content("Invoices");
}
