using System;
using System.ComponentModel.DataAnnotations;

namespace Amusing.Helpers;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
public sealed class RequiredIfActiveAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;

    public RequiredIfActiveAttribute(string dependentProperty)
    {
        _dependentProperty = dependentProperty;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var dependentProp = validationContext.ObjectType.GetProperty(_dependentProperty);
        if (dependentProp is null)
        {
            return new ValidationResult($"Property '{_dependentProperty}' not found.");
        }

        var dependentValue = dependentProp.GetValue(validationContext.ObjectInstance);
        int activeFlag = 0;

        if (dependentValue is int i) activeFlag = i;
        else if (dependentValue is bool b) activeFlag = b ? 1 : 0;
        else if (dependentValue is string s && int.TryParse(s, out var p)) activeFlag = p;

        // When Active == 1 require a non-empty value
        if (activeFlag == 1)
        {
            if (value is null) return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} is required.");
            if (value is string str && string.IsNullOrWhiteSpace(str)) return new ValidationResult(ErrorMessage ?? $"{validationContext.MemberName} is required.");
        }

        return ValidationResult.Success;
    }
}