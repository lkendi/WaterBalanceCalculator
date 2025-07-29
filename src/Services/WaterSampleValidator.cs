using System.Linq;
using WaterBalanceCalculator.Models;

namespace WaterBalanceCalculator.Services;

/// <summary>
/// Validates water sample to ensure proper calculation conditions.
/// </summary>

public static class WaterSampleValidator
{
    private static readonly string[] PropertyNames =
    {
        nameof(WaterSample.Calcium),
        nameof(WaterSample.Magnesium),
        nameof(WaterSample.Sodium),
        nameof(WaterSample.Potassium),
        nameof(WaterSample.Chloride),
        nameof(WaterSample.Fluoride),
        nameof(WaterSample.Nitrate),
        nameof(WaterSample.Sulfate),
        nameof(WaterSample.TotalAlkalinity),
        nameof(WaterSample.Conductivity)
    };

    public static ValidationResult ValidateForCalculation(WaterSample sample)
    {
        if (sample == null)
            return new ValidationResult(false, "Water sample cannot be null.");

        var values = GetSampleProperties(sample);

        var nullProperties = values.Where(p => p.Value == null).ToList();
        var negativeProperties = values.Where(p => p.Value < 0).ToList();

        if (negativeProperties.Any())
        {
            var negativeNames = string.Join(", ", negativeProperties.Select(p => p.Name));
            return new ValidationResult(false, $"Negative values not allowed: {negativeNames}");
        }

        if (nullProperties.Count == 0)
        {
            return new ValidationResult(false, "All values are provided. At least one value must be unknown for calculation.");
        }

        if (nullProperties.Count > 1)
        {
            var unknownNames = string.Join(", ", nullProperties.Select(p => p.Name));
            return new ValidationResult(false, $"Multiple unknown values found. Only one value can be unknown at a time.");
        }

        var unknownProperty = nullProperties.First().Name;
        return new ValidationResult(true, $"Valid for calculation. Unknown property: {unknownProperty}");
    }

    public static string? GetUnknownProperty(WaterSample sample)
    {
        var validationResult = ValidateForCalculation(sample);
        if (!validationResult.IsValid)
            return null;

        var values = GetSampleProperties(sample);
        return values.FirstOrDefault(p => p.Value == null).Name;
    }

    private static (string Name, double? Value)[] GetSampleProperties(WaterSample sample)
    {
        return new (string, double?)[]
        {
            (nameof(sample.Calcium), sample.Calcium),
            (nameof(sample.Magnesium), sample.Magnesium),
            (nameof(sample.Sodium), sample.Sodium),
            (nameof(sample.Potassium), sample.Potassium),
            (nameof(sample.Chloride), sample.Chloride),
            (nameof(sample.Fluoride), sample.Fluoride),
            (nameof(sample.Nitrate), sample.Nitrate),
            (nameof(sample.Sulfate), sample.Sulfate),
            (nameof(sample.TotalAlkalinity), sample.TotalAlkalinity),
            (nameof(sample.Conductivity), sample.Conductivity)
        };
    }
}
public record ValidationResult(bool IsValid, string Message);
