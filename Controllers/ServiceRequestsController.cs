using GLMS.Data;
using GLMS.Models;
using GLMS.Services;
using GLMS.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace GLMS.Controllers;

public class ServiceRequestsController(GlmsDbContext db, ICurrencyService currency) : Controller
{
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await db.ServiceRequests.AsNoTracking().Include(r => r.Contract).ThenInclude(c => c!.Client)
            .OrderByDescending(r => r.CreatedUtc).ToListAsync(cancellationToken));

    public async Task<IActionResult> Create(CancellationToken cancellationToken)
    {
        await ExpirePastDueContractsAsync(cancellationToken);
        await LoadContractsAsync(cancellationToken);
        await LoadExchangeRateAsync(cancellationToken);
        return View(new ServiceRequestCreateViewModel());
    }

    [HttpGet]
    public async Task<IActionResult> Rate(CancellationToken cancellationToken)
    {
        try
        {
            return Json(new { usdToZar = await currency.GetUsdToZarRateAsync(cancellationToken) });
        }
        catch (HttpRequestException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "The currency provider is unavailable." });
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "The currency provider timed out." });
        }
        catch (InvalidOperationException)
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "The currency provider returned an invalid exchange rate." });
        }
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ServiceRequestCreateViewModel model, CancellationToken cancellationToken)
    {
        await ExpirePastDueContractsAsync(cancellationToken);
        var contract = await db.Contracts.SingleOrDefaultAsync(c => c.Id == model.ContractId, cancellationToken);
        if (contract is null)
            ModelState.AddModelError(nameof(model.ContractId), "Select an existing contract.");
        else if (!ContractRules.CanCreateServiceRequest(contract.Status))
            ModelState.AddModelError(nameof(model.ContractId), $"Requests cannot be raised against a {contract.Status} contract.");

        if (!ModelState.IsValid)
        {
            await LoadContractsAsync(cancellationToken);
            await LoadExchangeRateAsync(cancellationToken);
            return View(model);
        }

        decimal rate;
        try
        {
            rate = await currency.GetUsdToZarRateAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Unable to retrieve the current USD/ZAR rate. No request was saved.");
            await LoadContractsAsync(cancellationToken);
            await LoadExchangeRateAsync(cancellationToken);
            return View(model);
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            ModelState.AddModelError(string.Empty, "The USD/ZAR rate request timed out. No request was saved.");
            await LoadContractsAsync(cancellationToken);
            await LoadExchangeRateAsync(cancellationToken);
            return View(model);
        }
        catch (InvalidOperationException)
        {
            ModelState.AddModelError(string.Empty, "The currency provider returned an invalid exchange rate. No request was saved.");
            await LoadContractsAsync(cancellationToken);
            await LoadExchangeRateAsync(cancellationToken);
            return View(model);
        }

        db.ServiceRequests.Add(new ServiceRequest
        {
            ContractId = model.ContractId,
            Description = model.Description,
            AmountUsd = model.AmountUsd,
            ExchangeRate = rate,
            CostZar = currency.ConvertUsdToZar(model.AmountUsd, rate),
            Status = "Submitted"
        });
        await db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index));
    }

    private async Task LoadContractsAsync(CancellationToken cancellationToken)
    {
        var contracts = await db.Contracts.AsNoTracking().Include(c => c.Client)
            .Where(c => c.Status != ContractStatus.Expired && c.Status != ContractStatus.OnHold)
            .OrderBy(c => c.Client!.Name).ToListAsync(cancellationToken);
        ViewBag.ContractOptions = contracts;
    }

    private async Task LoadExchangeRateAsync(CancellationToken cancellationToken)
    {
        try
        {
            ViewBag.ExchangeRate = await currency.GetUsdToZarRateAsync(cancellationToken);
        }
        catch (HttpRequestException)
        {
            ViewBag.RateError = "The live exchange rate is temporarily unavailable. Please retry shortly.";
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            ViewBag.RateError = "The exchange-rate request timed out. Please retry shortly.";
        }
        catch (InvalidOperationException)
        {
            ViewBag.RateError = "The currency provider returned an invalid exchange rate.";
        }
    }

    private async Task ExpirePastDueContractsAsync(CancellationToken cancellationToken)
    {
        var pastDue = await db.Contracts.Where(c => c.Status == ContractStatus.Active && c.EndDate < DateTime.Today)
            .ToListAsync(cancellationToken);
        if (pastDue.Count == 0) return;
        foreach (var contract in pastDue) contract.Status = ContractStatus.Expired;
        await db.SaveChangesAsync(cancellationToken);
    }
}
