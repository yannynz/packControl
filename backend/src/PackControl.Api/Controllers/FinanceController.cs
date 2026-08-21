using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PackControl.Application.Finance;

namespace PackControl.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/finance")]
public sealed class FinanceController(IFinanceService financeService) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<IActionResult> Overview(CancellationToken cancellationToken)
    {
        var overview = await financeService.GetOverviewAsync(cancellationToken);
        return Ok(overview);
    }

    [HttpPost("entries")]
    public async Task<IActionResult> CreateEntry([FromBody] CreateFinanceEntryRequest request, CancellationToken cancellationToken)
    {
        var overview = await financeService.CreateEntryAsync(request, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("entries/{entryId:guid}/settle")]
    public async Task<IActionResult> Settle(Guid entryId, CancellationToken cancellationToken)
    {
        var overview = await financeService.SettleAsync(entryId, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("entries/{entryId:guid}/boleto")]
    public async Task<IActionResult> GenerateBoleto(Guid entryId, CancellationToken cancellationToken)
    {
        var overview = await financeService.GenerateBoletoAsync(entryId, cancellationToken);
        return Ok(overview);
    }

    [HttpPost("invoices/issue")]
    public async Task<IActionResult> IssueInvoice([FromBody] IssueFiscalInvoiceRequest request, CancellationToken cancellationToken)
    {
        var overview = await financeService.IssueInvoiceAsync(request, cancellationToken);
        return Ok(overview);
    }
}
