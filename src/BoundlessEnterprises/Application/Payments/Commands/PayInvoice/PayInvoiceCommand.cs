using BoundlessEnterprises.Application.Common.Exceptions;
using BoundlessEnterprises.Application.Common.Interfaces;
using BoundlessEnterprises.Application.Payments.DTOs;
using BoundlessEnterprises.Domain.Payments;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BoundlessEnterprises.Application.Payments.Commands.PayInvoice;

/// <summary>
/// Pays an invoice by routing the charge through the company's payment
/// configuration, recording the result on the company's ledger, and marking the
/// invoice paid on success.
/// </summary>
public record PayInvoiceCommand(Guid CompanyId, Guid InvoiceId) : IRequest<PaymentDto>;

public class PayInvoiceHandler : IRequestHandler<PayInvoiceCommand, PaymentDto>
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IPaymentGateway _gateway;
    private readonly IDateTimeProvider _clock;

    public PayInvoiceHandler(
        IApplicationDbContext db,
        ICurrentUser currentUser,
        IPaymentGateway gateway,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _gateway = gateway;
        _clock = clock;
    }

    public async Task<PaymentDto> Handle(PayInvoiceCommand request, CancellationToken cancellationToken)
    {
        if (!_currentUser.CanAccessCompany(request.CompanyId))
            throw new ForbiddenAccessException();

        var invoice = await _db.Invoices
            .Include(i => i.Items)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId && i.CompanyId == request.CompanyId, cancellationToken)
            ?? throw new NotFoundException("Invoice", request.InvoiceId);

        if (invoice.Status == InvoiceStatus.Paid)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["invoice"] = new[] { "This invoice is already paid." },
            });
        if (invoice.Status == InvoiceStatus.Void)
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["invoice"] = new[] { "This invoice has been voided." },
            });

        var customer = await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == invoice.CustomerId, cancellationToken)
            ?? throw new NotFoundException("Customer", invoice.CustomerId);

        var payment = Payment.Create(request.CompanyId, customer.Id, invoice.Id, invoice.Total, invoice.Currency);
        _db.Payments.Add(payment);

        var result = await _gateway.ChargeAsync(
            new GatewayChargeRequest(request.CompanyId, invoice.Total, invoice.Currency,
                customer.Email, $"Invoice {invoice.Number}"),
            cancellationToken);

        if (result.Succeeded)
        {
            payment.MarkSucceeded(result.ExternalId, result.Method, _clock.UtcNow);
            invoice.MarkPaid(_clock.UtcNow);
        }
        else
        {
            payment.MarkFailed(_clock.UtcNow);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return PaymentDto.FromEntity(payment);
    }
}
