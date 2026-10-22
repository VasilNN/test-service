using TestService.Api.Emulators.Transactions;

namespace TestService.Api.Emulators.Outcomes;

/// <summary>Approves the transaction (also a capture of an authorized one); optionally notifies.</summary>
public sealed class ApproveOutcome(StatusChange change, TimeSpan? notifyAfter = null) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.Approve(change, context.Now);
        context.Notify(transaction, notifyAfter);
    }
}

/// <summary>Rejects the transaction (declines, timeouts, cancellations); optionally notifies.</summary>
public sealed class DeclineOutcome(StatusChange change, TimeSpan? notifyAfter = null) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.Reject(change, context.Now);
        context.Notify(transaction, notifyAfter);
    }
}

/// <summary>Puts the transaction in status <c>error</c>; optionally notifies.</summary>
public sealed class ErrorOutcome(StatusChange change, TimeSpan? notifyAfter = null) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.Fail(change, context.Now);
        context.Notify(transaction, notifyAfter);
    }
}

/// <summary>Authorizes the transaction; a later capture or cancel decides it.</summary>
public sealed class AuthorizeOutcome(StatusChange change) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context) => transaction.Authorize(change, context.Now);
}

/// <summary>Partially approves the transaction; optionally notifies.</summary>
public sealed class PartialApproveOutcome(StatusChange change, TimeSpan? notifyAfter = null) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.PartiallyApprove(change, context.Now);
        context.Notify(transaction, notifyAfter);
    }
}

/// <summary>Puts the transaction in status <c>verification</c>; optionally notifies.</summary>
public sealed class VerificationOutcome(StatusChange change, TimeSpan? notifyAfter = null) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.RequestVerification(change, context.Now);
        context.Notify(transaction, notifyAfter);
    }
}

/// <summary>
/// Leaves the transaction pending; the follow-up (if any) applies on the next status check.
/// Without a follow-up the transaction stays pending.
/// </summary>
public sealed class PendingUntilStatusCheckOutcome(StatusChange change, string? followUp) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.MarkPending(change, context.Now);
        if (followUp is not null)
        {
            transaction.SetStatusCheckFollowUp(followUp);
        }
    }
}

/// <summary>
/// Keeps the transaction <c>initialized</c> while the customer decides (for example on the 3DS
/// page); the follow-up applies when nobody decides before the scaled timeout.
/// </summary>
public sealed class AwaitDecisionOutcome(StatusChange change, TimeSpan timeout, string followUp) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context) =>
        transaction.AwaitAuthentication(change, context.Deadline(timeout, followUp), context.Now);
}

/// <summary>Leaves the transaction pending (optionally notifying); the follow-up applies after the scaled timeout.</summary>
public sealed class PendingThenTimeoutOutcome(StatusChange change, TimeSpan timeout, string followUp, TimeSpan? notifyAfter = null) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.MarkPending(change, context.Now);
        transaction.SetDeadline(context.Deadline(timeout, followUp));
        context.Notify(transaction, notifyAfter);
    }
}

/// <summary>Approves with a final amount computed from the requested one; the processed amount stays the requested one.</summary>
public sealed class ChangeAmountOutcome(Func<long, long> finalAmount, string code, string details, TimeSpan? notifyAfter = null) : IOutcome
{
    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        transaction.Approve(new StatusChange(code, details, "successful")
        {
            FinalAmount = finalAmount(transaction.RequestedAmount)
        }, context.Now);
        context.Notify(transaction, notifyAfter);
    }
}

/// <summary>Applies several outcomes in order, each recording its own notification (for example two notifications 5 s apart).</summary>
public sealed class NotificationSequenceOutcome(params IOutcome[] steps) : IOutcome
{
    public IReadOnlyList<IOutcome> Steps { get; } = steps;

    public void Apply(EmulatedTransaction transaction, OutcomeContext context)
    {
        foreach (var step in Steps)
        {
            step.Apply(transaction, context);
        }
    }
}
