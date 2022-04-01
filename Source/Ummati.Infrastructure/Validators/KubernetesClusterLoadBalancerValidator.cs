namespace Ummati.Infrastructure.Validators;

using FluentValidation;
using Ummati.Infrastructure.Configuration;

public class KubernetesClusterLoadBalancerValidator : AbstractValidator<KubernetesClusterLoadBalancer>
{
    public KubernetesClusterLoadBalancerValidator()
    {
        this.RuleFor(x => x.IdleTimeoutInMinutes).InclusiveBetween(4, 120);
        this.RuleFor(x => x.PortsPerNode).InclusiveBetween(0, 64_000);
    }
}
