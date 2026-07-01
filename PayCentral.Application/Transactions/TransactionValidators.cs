using FluentValidation;

namespace PayCentral.Application.Transactions;

public class LoadFundsCommandValidator : AbstractValidator<LoadFundsCommand>
{
    public LoadFundsCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("CardId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero")
            .LessThanOrEqualTo(50000).WithMessage("Maximum load amount is R50,000");
    }
}

public class PurchaseCommandValidator : AbstractValidator<PurchaseCommand>
{
    public PurchaseCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("CardId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");
    }
}

public class RefundCommandValidator : AbstractValidator<RefundCommand>
{
    public RefundCommandValidator()
    {
        RuleFor(x => x.CardId)
            .NotEmpty().WithMessage("CardId is required");

        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than zero");

        RuleFor(x => x.OriginalReferenceNumber)
            .NotEmpty().WithMessage("Original reference number is required");
    }
}