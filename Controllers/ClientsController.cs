using GLMS.Data;
using GLMS.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Controllers;

public class ClientsController(GlmsDbContext db) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await db.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken));

    public IActionResult Create() => View();

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Client client, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(client);
        db.Clients.Add(client);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var client = await db.Clients.FindAsync([id], cancellationToken);
        return client is null ? NotFound() : View(client);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, Client client, CancellationToken cancellationToken)
    {
        if (id != client.Id) return BadRequest();
        if (!ModelState.IsValid) return View(client);
        db.Entry(client).State = EntityState.Modified;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }
}
