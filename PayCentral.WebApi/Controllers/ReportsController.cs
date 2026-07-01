using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayCentral.Application.Common.Interfaces;
using PayCentral.Application.Reports;
using System.Text;

namespace PayCentral.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICsvExportService _csvExport;

    public ReportsController(IMediator mediator, ICsvExportService csvExport)
    {
        _mediator = mediator;
        _csvExport = csvExport;
    }

    [HttpGet("transactions")]
    public async Task<IActionResult> TransactionReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] Guid? cardId,
        [FromQuery] string format = "json")
    {
        var result = await _mediator.Send(
            new GetTransactionReportQuery(fromDate, toDate, cardId, format));

        if (format.ToLower() == "csv")
            return File(
                Encoding.UTF8.GetBytes(_csvExport.Export(result.Data!)),
                "text/csv",
                $"transactions_{DateTime.UtcNow:yyyyMMdd}.csv");

        return Ok(result);
    }

    [HttpGet("fraud")]
    public async Task<IActionResult> FraudReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] string format = "json")
    {
        var result = await _mediator.Send(
            new GetFraudReportQuery(fromDate, toDate, format));

        if (format.ToLower() == "csv")
            return File(
                Encoding.UTF8.GetBytes(_csvExport.Export(result.Data!)),
                "text/csv",
                $"fraud_{DateTime.UtcNow:yyyyMMdd}.csv");

        return Ok(result);
    }

    [HttpGet("cards")]
    public async Task<IActionResult> CardReport(
        [FromQuery] string? status,
        [FromQuery] string format = "json")
    {
        var result = await _mediator.Send(
            new GetCardReportQuery(status, format));

        if (format.ToLower() == "csv")
            return File(
                Encoding.UTF8.GetBytes(_csvExport.Export(result.Data!)),
                "text/csv",
                $"cards_{DateTime.UtcNow:yyyyMMdd}.csv");

        return Ok(result);
    }

    [HttpGet("daily-summary")]
    public async Task<IActionResult> DailySummary(
        [FromQuery] DateTime? date)
    {
        var result = await _mediator.Send(
            new GetDailySummaryQuery(date ?? DateTime.UtcNow));

        return Ok(result);
    }
}