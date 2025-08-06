using System.Linq;
using WaterBalanceCalculator.Models;

namespace WaterBalanceCalculator.Services;

/// <summary>
/// Validates water sample to ensure proper calculation conditions.
/// </summary>

public enum CalculationMode
{
    SingleUnknown,
    CationsOnly,
    AnionsOnly,
    CationsAndAnions
}

public record ValidationResult(bool IsValid, string Message, CalculationMode? Mode = null, string? UnknownProperty = null, string? SecondUnknownProperty = null);

public static class WaterSampleValidator
{
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

        if (IsCationsAndAnionsMode(sample, out string? cationUnknown, out string? anionUnknown))
        {
            return new ValidationResult(
                true,
                $"Valid for cations+anions calculation",
                CalculationMode.CationsAndAnions,
                cationUnknown,
                anionUnknown
            );
        }

        if (IsCationsOnlyMode(sample, out string? cationOnlyUnknown))
        {
            return new ValidationResult(
                true,
                $"Valid for cations-only calculation",
                CalculationMode.CationsOnly,
                cationOnlyUnknown
            );
        }

        if (IsAnionsOnlyMode(sample, out string? anionOnlyUnknown))
        {
            return new ValidationResult(
                true,
                $"Valid for anions-only calculation",
                CalculationMode.AnionsOnly,
                anionOnlyUnknown
            );
        }

        if (nullProperties.Count == 1)
        {
            return new ValidationResult(
                true,
                $"Valid for calculation",
                CalculationMode.SingleUnknown,
                nullProperties[0].Name
            );
        }

        if (nullProperties.Count == 0 && !sample.Conductivity.HasValue)
        {
            return new ValidationResult(
                true,
                $"Valid for conductivity calculation",
                CalculationMode.SingleUnknown,
                nameof(WaterSample.Conductivity)
            );
        }

        if (nullProperties.Count == 0)
        {
            return new ValidationResult(false, "All values provided. Leave one blank.");
        }

        return new ValidationResult(false, "Invalid input combination. Check guidance.");
    }

    private static bool IsCationsAndAnionsMode(WaterSample sample, out string? cationUnknown, out string? anionUnknown)
    {
        cationUnknown = null;
        anionUnknown = null;

        if (!sample.Conductivity.HasValue) return false;

        var cations = new (string Name, double? Value)[]
        {
            (nameof(sample.Calcium), sample.Calcium),
            (nameof(sample.Magnesium), sample.Magnesium),
            (nameof(sample.Sodium), sample.Sodium),
            (nameof(sample.Potassium), sample.Potassium)
        };

        var anions = new (string Name, double? Value)[]
        {
            (nameof(sample.Chloride), sample.Chloride),
            (nameof(sample.Fluoride), sample.Fluoride),
            (nameof(sample.Nitrate), sample.Nitrate),
            (nameof(sample.Sulfate), sample.Sulfate),
            (nameof(sample.TotalAlkalinity), sample.TotalAlkalinity)
        };

        var nullCations = cations.Where(c => !c.Value.HasValue).ToList();
        var nullAnions = anions.Where(a => !a.Value.HasValue).ToList();

        if (nullCations.Count != 1 || nullAnions.Count != 1) return false;

        cationUnknown = nullCations[0].Name;
        anionUnknown = nullAnions[0].Name;
        return true;
    }

    private static bool IsCationsOnlyMode(WaterSample sample, out string? unknownProperty)
    {
        unknownProperty = null;
        if (!sample.Conductivity.HasValue) return false;

        var cations = new (string Name, double? Value)[]
        {
            (nameof(sample.Calcium), sample.Calcium),
            (nameof(sample.Magnesium), sample.Magnesium),
            (nameof(sample.Sodium), sample.Sodium),
            (nameof(sample.Potassium), sample.Potassium)
        };

        var nullCations = cations.Count(c => !c.Value.HasValue);
        if (nullCations != 1) return false;

        if (sample.Chloride.HasValue || sample.Fluoride.HasValue ||
            sample.Nitrate.HasValue || sample.Sulfate.HasValue ||
            sample.TotalAlkalinity.HasValue) return false;

        unknownProperty = cations.First(c => !c.Value.HasValue).Name;
        return true;
    }

    private static bool IsAnionsOnlyMode(WaterSample sample, out string? unknownProperty)
    {
        unknownProperty = null;
        if (!sample.Conductivity.HasValue) return false;

        var anions = new (string Name, double? Value)[]
        {
            (nameof(sample.Chloride), sample.Chloride),
            (nameof(sample.Fluoride), sample.Fluoride),
            (nameof(sample.Nitrate), sample.Nitrate),
            (nameof(sample.Sulfate), sample.Sulfate),
            (nameof(sample.TotalAlkalinity), sample.TotalAlkalinity)
        };

        var nullAnions = anions.Count(a => !a.Value.HasValue);
        if (nullAnions != 1) return false;

        if (sample.Calcium.HasValue || sample.Magnesium.HasValue ||
            sample.Sodium.HasValue || sample.Potassium.HasValue) return false;

        unknownProperty = anions.First(a => !a.Value.HasValue).Name;
        return true;
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
