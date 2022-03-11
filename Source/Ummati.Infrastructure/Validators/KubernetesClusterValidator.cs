namespace Ummati.Infrastructure.Validators;
using System.Collections.Generic;
using FluentValidation;
using Ummati.Infrastructure.Configuration;

internal class KubernetesClusterValidator : AbstractValidator<KubernetesCluster>
{
    private const string ContainAtLeastOneSystemNodePoolMessage =
        "'{PropertyName}' must contain at least one system node pool.";

    public KubernetesClusterValidator()
    {
        this.RuleFor(x => x.Maintenance)
            .NotNull()
            .ForEach(x => x.SetValidator(new KubernetesClusterMaintenanceValidator()));
        this.RuleFor(x => x.SKUTier).IsInEnum();
        this.RuleFor(x => x.UpgradeChannel).IsInEnum();
        this.RuleFor(x => x.NodePools)
            .NotNull()
            .NotEmpty()
            .Must(ContainAtLeastOneSystemNodePool)
            .WithMessage(ContainAtLeastOneSystemNodePoolMessage)
            .ForEach(x => x.SetValidator(new KubernetesClusterNodePoolValidator()));
    }

    private static bool ContainAtLeastOneSystemNodePool(IEnumerable<KubernetesClusterNodePool> nodePools) =>
        nodePools.Any(x => x.Type == KubernetesClusterNodePoolType.System);
}
