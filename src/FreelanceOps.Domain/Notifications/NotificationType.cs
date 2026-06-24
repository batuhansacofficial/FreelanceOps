namespace FreelanceOps.Domain.Notifications;

public enum NotificationType
{
    ProposalSent = 1,
    ProposalAccepted = 2,
    ProposalConvertedToProject = 3,

    InvoiceSent = 10,
    InvoicePaid = 11,
    InvoiceOverdue = 12,

    ProposalExpired = 20,

    TaskAssigned = 30
}
