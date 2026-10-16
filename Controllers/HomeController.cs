using GLMS.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Controllers;

public class HomeController(GlmsDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        ViewBag.ClientCount = await db.Clients.CountAsync(cancellationToken);
        ViewBag.ContractCount = await db.Contracts.CountAsync(cancellationToken);
        ViewBag.RequestCount = await db.ServiceRequests.CountAsync(cancellationToken);
        return View();
    }

    public IActionResult Error() => View();
}
