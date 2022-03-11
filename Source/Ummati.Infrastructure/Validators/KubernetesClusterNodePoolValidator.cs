namespace Ummati.Infrastructure.Validators;

using FluentValidation;
using Ummati.Infrastructure.Configuration;

public class KubernetesClusterNodePoolValidator : AbstractValidator<KubernetesClusterNodePool>
{
    private const string NullForSpotNodePoolsMessage = "'{PropertyName}' must be null for spot node pools.";
    private const string BeANumberOrPercentageMessage = "'{PropertyName}' must be a number or percentage.";

    public KubernetesClusterNodePoolValidator()
    {
        this.RuleFor(x => x.AvailabilityZones)
            .Null()
            .When(x => x.Type == KubernetesClusterNodePoolType.Spot)
            .WithMessage(NullForSpotNodePoolsMessage)
            .ForEach(x => x.InclusiveBetween(1, 3));
        this.RuleFor(x => x.MaximumNodeCount)
            .LessThanOrEqualTo(100)
            .GreaterThanOrEqualTo(x => x.MinimumNodeCount);
        this.RuleFor(x => x.MaximumPods).InclusiveBetween(1, 250);
        this.RuleFor(x => x.MaximumSurge)
            .NotNull()
            .NotEmpty()
            .Matches(@"^(\d+)(\%?)$")
            .WithMessage(BeANumberOrPercentageMessage);
        this.RuleFor(x => x.MinimumNodeCount)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(x => x.MaximumNodeCount);
        this.RuleFor(x => x.OsDiskSizeGB).GreaterThanOrEqualTo(1);
        this.RuleFor(x => x.OSDiskType).IsInEnum();
        this.RuleFor(x => x.ScaleSetEvictionPolicy).IsInEnum();
        this.RuleFor(x => x.Type).IsInEnum();
        this.RuleFor(x => x.VmSize).NotNull().NotEmpty();
    }
}
