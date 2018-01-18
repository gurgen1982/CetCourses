using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

[AttributeUsage(AttributeTargets.Property, Inherited = true, AllowMultiple = false)]
public sealed class IsGreaterAttribute : ValidationAttribute, IClientValidatable
{
    private readonly string compareWithPropertyName;
    private readonly bool allowEqual;


    public IsGreaterAttribute(string compareWithPropertyName, bool allowEqual = false)
    {
        ErrorMessage = "Must be greater than {0}";
        this.compareWithPropertyName = compareWithPropertyName;
        this.allowEqual = allowEqual;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var propertyTestedInfo = validationContext.ObjectType.GetProperty(this.compareWithPropertyName);
        if (propertyTestedInfo == null)
        {
            return new ValidationResult(string.Format("unknown property {0}", this.compareWithPropertyName));
        }

        var propertyTestedValue = propertyTestedInfo.GetValue(validationContext.ObjectInstance, null);

        if (value == null || !(value is int))
        {
            return ValidationResult.Success;
        }

        if (propertyTestedValue == null || !(propertyTestedValue is int))
        {
            return ValidationResult.Success;
        }

        // Compare values
        if ((int)value >= (int)propertyTestedValue)
        {
            if (this.allowEqual && (int)value == (int)propertyTestedValue)
            {
                return ValidationResult.Success;
            }
            else if ((int)value > (int)propertyTestedValue)
            {
                return ValidationResult.Success;
            }
        }
        
        return new ValidationResult(FormatErrorMessage(validationContext.DisplayName));
    }

    public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
    {
        var rule = new ModelClientValidationRule
        {
            ErrorMessage = string.Format(this.ErrorMessageString, this.compareWithPropertyName),
            ValidationType = "isgreaterthan"
        };
        rule.ValidationParameters["comparewith"] = this.compareWithPropertyName;
        rule.ValidationParameters["allowequal"] = this.allowEqual;
        yield return rule;
    }
}
 