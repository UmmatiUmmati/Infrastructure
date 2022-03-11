namespace Ummati.Infrastructure.Validators;

using System;
using FluentValidation;
using Ummati.Infrastructure.Configuration;

public class ConfigurationValidator : AbstractValidator<IConfiguration>
{
    private const string AzureLocationMessage = "'{PropertyName}' with value '{PropertyValue}' must be an Azure location.";
    private const string LowercaseMessage = "'{PropertyName}' with value '{PropertyValue}' must be lowercase.";

    public ConfigurationValidator()
    {
        this.RuleFor(x => x.ApplicationName)
            .NotNull()
            .NotEmpty()
            .Must(BeLowerCase)
            .WithMessage(LowercaseMessage);
        this.RuleFor(x => x.Environment)
            .NotNull()
            .NotEmpty()
            .Must(BeLowerCase)
            .WithMessage(LowercaseMessage);
        this.RuleFor(x => x.CommonLocation)
            .NotNull()
            .NotEmpty()
            .Must(BeLowerCase)
            .WithMessage(LowercaseMessage)
            .Must(BeAnAzureLocation)
            .WithMessage(AzureLocationMessage);
        this.RuleFor(x => x.Locations)
            .NotNull()
            .ForEach(x => x
                .Must(BeLowerCase)
                .WithMessage(LowercaseMessage)
                .Must(BeAnAzureLocation)
                .WithMessage(AzureLocationMessage));
        this.RuleFor(x => x.Kubernetes)
            .NotNull()
            .SetValidator(x => new KubernetesClusterValidator());
    }

    private static bool BeAnAzureLocation(string location) => Azure.Locations.Contains(location);

    private static bool BeLowerCase(string value) => value.All(x => char.IsLower(x));
}
