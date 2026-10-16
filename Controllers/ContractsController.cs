using GLMS.Data;
using GLMS.Models;
using GLMS.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Controllers;

public class ContractsController(GlmsDbContext db, IAgreementStorage storage) : Controller
{
    private const long MaxAgreementSizeBytes = 20 * 1024 * 1024;

    public async Task<IActionResult> Index(ContractStatus? status, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        await ExpirePastDueContractsAsync(cancellationToken);
        var query = db.Contracts.AsNoTracking().Include(c => c.Client).AsQueryable();
        if (status.HasValue) query = query.Where(c => c.Status == status.Value);
        if (from.HasValue) query = query.Where(c => c.EndDate >= from.Value.Date);
        if (to.HasValue) query = query.Where(c => c.StartDate <= to.Value.Date);
        ViewBag.Status = status;
        ViewBag.From = from?.ToString("yyyy-MM-dd");
        ViewBag.To = to?.ToString("yyyy-MM-dd");
        return View(await query.OrderBy(c => c.EndDate).ToListAsync(cancellationToken));
    }

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await LoadClientsAsync(cancellationToken);
        return View(new Contract());
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(21_000_000)]
    public async Task<IActionResult> Create([Bind("ClientId,StartDate,EndDate,Status,ServiceLevel")] Contract contract,
        IFormFile? agreement, CancellationToken cancellationToken)
    {
        if (contract.EndDate.Date < contract.StartDate.Date)
            ModelState.AddModelError(nameof(contract.EndDate), "End date must be on or after the start date.");
        if (!await db.Clients.AnyAsync(c => c.Id == contract.ClientId, cancellationToken))
            ModelState.AddModelError(nameof(contract.ClientId), "Select an existing client.");
        if (agreement is null || agreement.Length == 0)
            ModelState.AddModelError("agreement", "A signed PDF agreement is required.");
        else if (agreement.Length > MaxAgreementSizeBytes)
            ModelState.AddModelError("agreement", "The agreement must be 20 MB or smaller.");
        if (!ModelState.IsValid)
        {
            await LoadClientsAsync(cancellationToken);
            return View(contract);
        }

        try
        {
            await using var validationStream = agreement!.OpenReadStream();
            await AgreementFileValidator.ValidatePdfAsync(agreement.FileName, validationStream, cancellationToken);
            await using var input = agreement.OpenReadStream();
            contract.AgreementFileName = Path.GetFileName(agreement.FileName);
            contract.AgreementStorageKey = await storage.SaveAsync(agreement.FileName, input, cancellationToken);
        }
        catch (InvalidDataException ex)
        {
            ModelState.AddModelError("agreement", ex.Message);
            await LoadClientsAsync(cancellationToken);
            return View(contract);
        }

        db.Contracts.Add(contract);
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var contract = await db.Contracts.FindAsync([id], cancellationToken);
        if (contract is null) return NotFound();
        await LoadClientsAsync(cancellationToken);
        return View(contract);
    }

    [HttpPost, ValidateAntiForgeryToken, RequestSizeLimit(21_000_000)]
    public async Task<IActionResult> Edit(int id, [Bind("Id,ClientId,StartDate,EndDate,Status,ServiceLevel,AgreementFileName,AgreementStorageKey")] Contract contract,
        IFormFile? agreement, CancellationToken cancellationToken)
    {
        if (id != contract.Id) return BadRequest();
        if (contract.EndDate.Date < contract.StartDate.Date)
            ModelState.AddModelError(nameof(contract.EndDate), "End date must be on or after the start date.");
        if (!await db.Clients.AnyAsync(c => c.Id == contract.ClientId, cancellationToken))
            ModelState.AddModelError(nameof(contract.ClientId), "Select an existing client.");
        if (agreement is { Length: > MaxAgreementSizeBytes })
            ModelState.AddModelError("agreement", "The agreement must be 20 MB or smaller.");
        if (!ModelState.IsValid)
        {
            await LoadClientsAsync(cancellationToken);
            return View(contract);
        }

        var existing = await db.Contracts.SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (existing is null) return NotFound();
        if (agreement is { Length: > 0 })
        {
            try
            {
                await using var validationStream = agreement.OpenReadStream();
                await AgreementFileValidator.ValidatePdfAsync(agreement.FileName, validationStream, cancellationToken);
                await using var upload = agreement.OpenReadStream();
                existing.AgreementFileName = Path.GetFileName(agreement.FileName);
                existing.AgreementStorageKey = await storage.SaveAsync(agreement.FileName, upload, cancellationToken);
            }
            catch (InvalidDataException ex)
            {
                ModelState.AddModelError("agreement", ex.Message);
                await LoadClientsAsync(cancellationToken);
                return View(contract);
            }
        }

        existing.ClientId = contract.ClientId;
        existing.StartDate = contract.StartDate;
        existing.EndDate = contract.EndDate;
        existing.Status = contract.Status;
        existing.ServiceLevel = contract.ServiceLevel;
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var contract = await db.Contracts.AsNoTracking().SingleOrDefaultAsync(c => c.Id == id, cancellationToken);
        if (contract?.AgreementStorageKey is null) return NotFound();
        try
        {
            var stream = await storage.OpenReadAsync(contract.AgreementStorageKey, cancellationToken);
            return File(stream, "application/pdf", contract.AgreementFileName ?? "signed-agreement.pdf");
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task LoadClientsAsync(CancellationToken cancellationToken) =>
        ViewBag.Clients = new SelectList(await db.Clients.AsNoTracking().OrderBy(c => c.Name).ToListAsync(cancellationToken), "Id", "Name");

    private async Task ExpirePastDueContractsAsync(CancellationToken cancellationToken)
    {
        var pastDue = await db.Contracts.Where(c => c.Status == ContractStatus.Active && c.EndDate < DateTime.Today)
            .ToListAsync(cancellationToken);
        if (pastDue.Count == 0) return;
        foreach (var contract in pastDue) contract.Status = ContractStatus.Expired;
        await db.SaveChangesAsync(cancellationToken);
    }
}
