namespace Ummati.Infrastructure.Validators;

using FluentValidation;
using Ummati.Infrastructure.Configuration;

public class KubernetesClusterMaintenanceValidator : AbstractValidator<KubernetesClusterMaintenance>
{
    public KubernetesClusterMaintenanceValidator()
    {
        this.RuleFor(x => x.Days).NotNull().ForEach(x => x.IsInEnum());
        this.RuleFor(x => x.HourSlots).NotNull().ForEach(x => x.InclusiveBetween(0, 24));
    }
}
