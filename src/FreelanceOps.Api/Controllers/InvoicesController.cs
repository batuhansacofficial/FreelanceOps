using FreelanceOps.Application.Billing;
using FreelanceOps.Application.Billing.CancelInvoice;
using FreelanceOps.Application.Billing.CreateInvoice;
using FreelanceOps.Application.Billing.DeleteInvoice;
using FreelanceOps.Application.Billing.GetInvoiceById;
using FreelanceOps.Application.Billing.GetInvoicePayments;
using FreelanceOps.Application.Billing.GetInvoices;
using FreelanceOps.Application.Billing.RecordPayment;
using FreelanceOps.Application.Billing.SendInvoice;
using FreelanceOps.Application.Billing.UpdateInvoice;
using FreelanceOps.Application.Common.Pagination;
using FreelanceOps.Domain.Billing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FreelanceOps.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/workspaces/{workspaceId:guid}/invoices")]
public sealed class InvoicesController(
    CreateInvoiceHandler createInvoiceHandler,
    GetInvoicesHandler getInvoicesHandler,
    GetInvoiceByIdHandler getInvoiceByIdHandler,
    UpdateInvoiceHandler updateInvoiceHandler,
    DeleteInvoiceHandler deleteInvoiceHandler,
    SendInvoiceHandler sendInvoiceHandler,
    CancelInvoiceHandler cancelInvoiceHandler,
    RecordPaymentHandler recordPaymentHandler,
    GetInvoicePaymentsHandler getInvoicePaymentsHandler) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CreateInvoiceResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(
        Guid workspaceId,
        CreateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        var response = await createInvoiceHandler.Handle(
            new CreateInvoiceCommand(
                workspaceId,
                request.ClientId,
                request.ProjectId,
                request.IssueDate,
                request.DueDate,
                request.Currency,
                request.Notes,
                request.Items?.Select(ToInput).ToArray() ?? []),
            cancellationToken);

        return Created(
            $"/api/workspaces/{workspaceId}/invoices/{response.Id}",
            response);
    }

    [HttpGet]
    [ProducesResponseType<PagedResult<InvoiceSummaryResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        Guid workspaceId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] InvoiceStatus? status = null,
        [FromQuery] Guid? clientId = null,
        [FromQuery] Guid? projectId = null,
        [FromQuery] string? search = null,
        [FromQuery] DateOnly? fromIssueDate = null,
        [FromQuery] DateOnly? toIssueDate = null,
        CancellationToken cancellationToken = default)
    {
        var response = await getInvoicesHandler.Handle(
            new GetInvoicesQuery(
                workspaceId,
                page,
                pageSize,
                status,
                clientId,
                projectId,
                search,
                fromIssueDate,
                toIssueDate),
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("{invoiceId:guid}")]
    [ProducesResponseType<InvoiceDetailResponse>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(
        Guid workspaceId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var response = await getInvoiceByIdHandler.Handle(
            new GetInvoiceByIdQuery(workspaceId, invoiceId),
            cancellationToken);

        return Ok(response);
    }

    [HttpPut("{invoiceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Update(
        Guid workspaceId,
        Guid invoiceId,
        UpdateInvoiceRequest request,
        CancellationToken cancellationToken)
    {
        await updateInvoiceHandler.Handle(
            new UpdateInvoiceCommand(
                workspaceId,
                invoiceId,
                request.IssueDate,
                request.DueDate,
                request.Currency,
                request.Notes,
                request.Items?.Select(ToInput).ToArray() ?? []),
            cancellationToken);

        return NoContent();
    }

    [HttpDelete("{invoiceId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(
        Guid workspaceId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        await deleteInvoiceHandler.Handle(
            new DeleteInvoiceCommand(workspaceId, invoiceId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{invoiceId:guid}/send")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Send(
        Guid workspaceId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        await sendInvoiceHandler.Handle(
            new SendInvoiceCommand(workspaceId, invoiceId),
            cancellationToken);

        return NoContent();
    }

    [HttpPatch("{invoiceId:guid}/cancel")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Cancel(
        Guid workspaceId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        await cancelInvoiceHandler.Handle(
            new CancelInvoiceCommand(workspaceId, invoiceId),
            cancellationToken);

        return NoContent();
    }

    [HttpPost("{invoiceId:guid}/payments")]
    [ProducesResponseType<RecordPaymentResponse>(StatusCodes.Status201Created)]
    public async Task<IActionResult> RecordPayment(
        Guid workspaceId,
        Guid invoiceId,
        RecordPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var response = await recordPaymentHandler.Handle(
            new RecordPaymentCommand(
                workspaceId,
                invoiceId,
                request.Amount,
                request.Method,
                request.Reference,
                request.PaidAt),
            cancellationToken);

        return Created(
            $"/api/workspaces/{workspaceId}/invoices/{invoiceId}/payments/{response.Id}",
            response);
    }

    [HttpGet("{invoiceId:guid}/payments")]
    [ProducesResponseType<IReadOnlyCollection<PaymentRecordResponse>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        Guid workspaceId,
        Guid invoiceId,
        CancellationToken cancellationToken)
    {
        var response = await getInvoicePaymentsHandler.Handle(
            new GetInvoicePaymentsQuery(workspaceId, invoiceId),
            cancellationToken);

        return Ok(response);
    }

    private static InvoiceItemInput ToInput(InvoiceItemRequest item)
    {
        return new InvoiceItemInput(
            item.Description,
            item.Quantity,
            item.UnitPrice,
            item.TaxRate);
    }
}

public sealed record CreateInvoiceRequest(
    Guid ClientId,
    Guid? ProjectId,
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    string? Notes,
    IReadOnlyCollection<InvoiceItemRequest> Items);

public sealed record UpdateInvoiceRequest(
    DateOnly IssueDate,
    DateOnly DueDate,
    string Currency,
    string? Notes,
    IReadOnlyCollection<InvoiceItemRequest> Items);

public sealed record InvoiceItemRequest(
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal TaxRate);

public sealed record RecordPaymentRequest(
    decimal Amount,
    PaymentMethod Method,
    string? Reference,
    DateOnly PaidAt);
