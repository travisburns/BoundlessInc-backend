using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Payments.Commands.PayInvoice;
using BoundlessEnterprises.Application.Payments.Queries.GetInvoices;
using BoundlessEnterprises.Application.Payments.Queries.GetPayments;
using BoundlessEnterprises.Application.Payments.Queries.GetSubscriptions;
using MediatR;

namespace BoundlessEnterprises.Api.Endpoints.Payments;

/// <summary>
/// Company-aware billing endpoints plus the payment-provider webhook. Invoices,
/// payments, and subscriptions are all routed and stored by CompanyId.
/// </summary>
public sealed class PaymentsEndpoints : IEndpointModule
{
    public void MapEndpoints(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/companies/{companyId:guid}/billing")
            .WithTags("Payments")
            .RequireAuthorization();

        group.MapGet("/invoices", async (Guid companyId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetInvoicesQuery(companyId), ct)))
            .WithName("GetInvoices")
            .WithSummary("List a company's invoices.");

        group.MapPost("/invoices/{invoiceId:guid}/pay",
            async (Guid companyId, Guid invoiceId, ISender sender, CancellationToken ct) =>
                Results.Ok(await sender.Send(new PayInvoiceCommand(companyId, invoiceId), ct)))
            .WithName("PayInvoice")
            .WithSummary("Pay an invoice through the company's payment configuration.");

        group.MapGet("/payments", async (Guid companyId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetPaymentsQuery(companyId), ct)))
            .WithName("GetPayments")
            .WithSummary("List a company's payment ledger.");

        group.MapGet("/subscriptions", async (Guid companyId, ISender sender, CancellationToken ct) =>
            Results.Ok(await sender.Send(new GetSubscriptionsQuery(companyId), ct)))
            .WithName("GetSubscriptions")
            .WithSummary("List a company's subscriptions.");

        // Provider webhook — anonymous; authenticity is verified by signature.
        app.MapPost("/api/payments/webhook",
            async (HttpRequest req, IPaymentWebhookHandler handler, CancellationToken ct) =>
            {
                using var reader = new StreamReader(req.Body);
                var payload = await reader.ReadToEndAsync(ct);
                var signature = req.Headers["Stripe-Signature"].FirstOrDefault();
                await handler.HandleAsync(payload, signature, ct);
                return Results.Ok();
            })
            .WithTags("Payments")
            .WithName("PaymentsWebhook")
            .WithSummary("Receive payment-provider webhook events.");
    }
}
