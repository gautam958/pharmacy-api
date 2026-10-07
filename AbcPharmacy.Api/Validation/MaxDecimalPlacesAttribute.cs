using System.ComponentModel.DataAnnotations;

namespace AbcPharmacy.Api.Validation;

public class MaxDecimalPlacesAttribute : ValidationAttribute
{
    private readonly int _places;

    public MaxDecimalPlacesAttribute(int places)
    {
        _places = places;
    }

    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is decimal number && decimal.Round(number, _places) != number)
        {
            return new ValidationResult($"{validationContext.DisplayName} can have at most {_places} decimal places.");
        }

        return ValidationResult.Success;
    }
}
