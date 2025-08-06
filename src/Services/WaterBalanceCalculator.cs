using System;
using WaterBalanceCalculator.Constants;
using WaterBalanceCalculator.Models;

namespace WaterBalanceCalculator.Services;

/// <summary>
/// Service for calculating water balance based on provided water sample.
/// It determines the unknown property based on the provided values and calculates its value.
/// </summary>

public static class WaterBalanceCalculatorService
{
    public static BalanceResult Calculate(WaterSample sample)
    {
        var validation = WaterSampleValidator.ValidateForCalculation(sample);
        if (!validation.IsValid)
        {
            return new BalanceResult(
                ErrorMessage: validation.Message,
                Status: "Invalid Input"
            );
        }

        try
        {
            return validation.Mode switch
            {
                CalculationMode.SingleUnknown => CalculateSingleUnknown(sample, validation.UnknownProperty),
                CalculationMode.CationsOnly => CalculateCationsOnly(sample, validation.UnknownProperty),
                CalculationMode.AnionsOnly => CalculateAnionsOnly(sample, validation.UnknownProperty),
                CalculationMode.CationsAndAnions => CalculateCationsAndAnions(sample, validation.UnknownProperty, validation.SecondUnknownProperty),
                _ => new BalanceResult(
                    ErrorMessage: "Unsupported calculation mode",
                    Status: "Calculation Error"
                )
            };
        }
        catch (Exception ex)
        {
            return new BalanceResult(
                ErrorMessage: $"Error: {ex.Message}",
                Status: "Calculation Error"
            );
        }
    }

    private static BalanceResult CalculateSingleUnknown(WaterSample sample, string? unknownProperty)
    {
        if (unknownProperty == nameof(WaterSample.Conductivity))
        {
            return CalculateConductivity(sample);
        }

        if (unknownProperty == null)
        {
            return new BalanceResult(
                ErrorMessage: "Unknown property cannot be empty",
                Status: "Invalid Input"
            );
        }
        return CalculateIon(sample, unknownProperty, "Calculation Complete");
    }

    private static BalanceResult CalculateCationsOnly(WaterSample sample, string? unknownProperty)
    {
        double totalCationsInMeq = sample.Conductivity!.Value / ChemicalConstants.ConductivityConversionFactor;

        if (unknownProperty == null)
        {
            return new BalanceResult(
                ErrorMessage: "Unknown property cannot be empty",
                Status: "Invalid Input"
            );
        }
        double knownCationsSum = CalculateCationsSumWithout(sample, unknownProperty);
        double unknownCationInMeq = totalCationsInMeq - knownCationsSum;
        double weight = GetMolarMass(unknownProperty);
        double value = unknownCationInMeq * weight;

        if (value < 0)
        {
            return new BalanceResult(
                ErrorMessage: $"Negative result for {unknownProperty}",
                Status: "Invalid Result"
            );
        }

        return new BalanceResult(
            CationsSum: totalCationsInMeq,
            AnionsSum: null,
            Status: "Calculation Complete (Cations only)",
            SolvedProperty: unknownProperty,
            SolvedValue: value
        );
    }

    private static BalanceResult CalculateAnionsOnly(WaterSample sample, string? unknownProperty)
    {
        double totalAnionsInMeq = sample.Conductivity!.Value / ChemicalConstants.ConductivityConversionFactor;

        if (unknownProperty == null)
        {
            return new BalanceResult(
                ErrorMessage: "Unknown property cannot be empty",
                Status: "Invalid Input"
            );
        }
        double knownAnionsSum = CalculateAnionsSumWithout(sample, unknownProperty);
        double unknownAnionInMeq = totalAnionsInMeq - knownAnionsSum;
        double weight = GetMolarMass(unknownProperty);
        double value = unknownAnionInMeq * weight;

        if (value < 0)
        {
            return new BalanceResult(
                ErrorMessage: $"Negative result for {unknownProperty}",
                Status: "Invalid Result"
            );
        }

        return new BalanceResult(
            CationsSum: null,
            AnionsSum: totalAnionsInMeq,
            Status: "Calculation Complete (Anions only)",
            SolvedProperty: unknownProperty,
            SolvedValue: value
        );
    }

    private static BalanceResult CalculateCationsAndAnions(WaterSample sample, string? cationProperty, string? anionProperty)
    {
        double totalInMeq = sample.Conductivity!.Value / ChemicalConstants.ConductivityConversionFactor;
        if (cationProperty == null || anionProperty == null)
        {
            return new BalanceResult(
                ErrorMessage: "Unknown properties cannot be empty",
                Status: "Invalid Input"
            );
        }

        double knownCationsSum = CalculateCationsSumWithout(sample, cationProperty);
        double cationWeight = GetMolarMass(cationProperty);
        double unknownCationInMeq = totalInMeq - knownCationsSum;
        double cationValue = unknownCationInMeq * cationWeight;

        double knownAnionsSum = CalculateAnionsSumWithout(sample, anionProperty);
        double anionWeight = GetMolarMass(anionProperty);
        double unknownAnionInMeq = totalInMeq - knownAnionsSum;
        double anionValue = unknownAnionInMeq * anionWeight;

        if (cationValue < 0 || anionValue < 0)
        {
            return new BalanceResult(
                ErrorMessage: "Negative result in calculation",
                Status: "Invalid Result"
            );
        }

        return new BalanceResult(
            CationsSum: totalInMeq,
            AnionsSum: totalInMeq,
            Status: "Calculation Complete (Cations and anions)",
            SolvedProperty: cationProperty,
            SolvedValue: cationValue,
            SecondSolvedProperty: anionProperty,
            SecondSolvedValue: anionValue
        );
    }

    private static BalanceResult CalculateIon(WaterSample sample, string propertyName, string status = "Calculation Complete")
    {
        double equivalentSum;
        double weight;
        bool isCation = false;

        switch (propertyName)
        {
            case nameof(WaterSample.Calcium):
                equivalentSum = CalculateAnionsSum(sample);
                equivalentSum -= CalculateCationsSumWithout(sample, propertyName);
                weight = ChemicalConstants.CalciumWeight;
                isCation = true;
                break;
            case nameof(WaterSample.Magnesium):
                equivalentSum = CalculateAnionsSum(sample);
                equivalentSum -= CalculateCationsSumWithout(sample, propertyName);
                weight = ChemicalConstants.MagnesiumWeight;
                isCation = true;
                break;
            case nameof(WaterSample.Sodium):
                equivalentSum = CalculateAnionsSum(sample);
                equivalentSum -= CalculateCationsSumWithout(sample, propertyName);
                weight = ChemicalConstants.SodiumWeight;
                isCation = true;
                break;
            case nameof(WaterSample.Potassium):
                equivalentSum = CalculateAnionsSum(sample);
                equivalentSum -= CalculateCationsSumWithout(sample, propertyName);
                weight = ChemicalConstants.PotassiumWeight;
                isCation = true;
                break;
            case nameof(WaterSample.Chloride):
                equivalentSum = CalculateCationsSum(sample);
                equivalentSum -= CalculateAnionsSumWithout(sample, propertyName);
                weight = ChemicalConstants.ChlorideWeight;
                break;
            case nameof(WaterSample.Fluoride):
                equivalentSum = CalculateCationsSum(sample);
                equivalentSum -= CalculateAnionsSumWithout(sample, propertyName);
                weight = ChemicalConstants.FluorideWeight;
                break;
            case nameof(WaterSample.Nitrate):
                equivalentSum = CalculateCationsSum(sample);
                equivalentSum -= CalculateAnionsSumWithout(sample, propertyName);
                weight = ChemicalConstants.NitrateWeight;
                break;
            case nameof(WaterSample.Sulfate):
                equivalentSum = CalculateCationsSum(sample);
                equivalentSum -= CalculateAnionsSumWithout(sample, propertyName);
                weight = ChemicalConstants.SulfateWeight;
                break;
            case nameof(WaterSample.TotalAlkalinity):
                equivalentSum = CalculateCationsSum(sample);
                equivalentSum -= CalculateAnionsSumWithout(sample, propertyName);
                weight = ChemicalConstants.AlkalinityWeight;
                break;
            default:
                throw new ArgumentException($"Unknown property: {propertyName}");
        }

        var value = equivalentSum * weight;
        if (value < 0)
        {
            return new BalanceResult(
                ErrorMessage: $"Negative result for {propertyName}",
                Status: "Invalid Result"
            );
        }

        var cationsSum = isCation ?
            CalculateCationsSumWithout(sample, propertyName) + equivalentSum :
            CalculateCationsSum(sample);

        var anionsSum = isCation ?
            CalculateAnionsSum(sample) :
            CalculateAnionsSumWithout(sample, propertyName) + equivalentSum;

        return new BalanceResult(
            CationsSum: cationsSum,
            AnionsSum: anionsSum,
            Status: status,
            SolvedProperty: propertyName,
            SolvedValue: value
        );
    }

    public static BalanceResult CalculateConductivity(WaterSample sample)
    {
        var cationsSum = CalculateCationsSum(sample);
        var anionsSum = CalculateAnionsSum(sample);

        var averageSum = (cationsSum + anionsSum) / 2;
        var conductivityValue = averageSum * ChemicalConstants.ConductivityConversionFactor;

        return new BalanceResult(
            CationsSum: cationsSum,
            AnionsSum: anionsSum,
            Status: "Calculation Complete",
            SolvedProperty: "Conductivity",
            SolvedValue: conductivityValue
        );
    }

    private static double CalculateCationsSum(WaterSample sample)
    {
        double sum = 0;
        if (sample.Calcium.HasValue)
            sum += sample.Calcium.Value / ChemicalConstants.CalciumWeight;
        if (sample.Magnesium.HasValue)
            sum += sample.Magnesium.Value / ChemicalConstants.MagnesiumWeight;
        if (sample.Sodium.HasValue)
            sum += sample.Sodium.Value / ChemicalConstants.SodiumWeight;
        if (sample.Potassium.HasValue)
            sum += sample.Potassium.Value / ChemicalConstants.PotassiumWeight;
        return sum;
    }

    private static double CalculateAnionsSum(WaterSample sample)
    {
        double sum = 0;
        if (sample.TotalAlkalinity.HasValue)
            sum += sample.TotalAlkalinity.Value / ChemicalConstants.AlkalinityWeight;
        if (sample.Chloride.HasValue)
            sum += sample.Chloride.Value / ChemicalConstants.ChlorideWeight;
        if (sample.Fluoride.HasValue)
            sum += sample.Fluoride.Value / ChemicalConstants.FluorideWeight;
        if (sample.Nitrate.HasValue)
            sum += sample.Nitrate.Value / ChemicalConstants.NitrateWeight;
        if (sample.Sulfate.HasValue)
            sum += sample.Sulfate.Value / ChemicalConstants.SulfateWeight;
        return sum;
    }

    private static double CalculateCationsSumWithout(WaterSample sample, string excludeProperty)
    {
        double sum = 0;

        if (excludeProperty != nameof(WaterSample.Calcium) && sample.Calcium.HasValue)
            sum += sample.Calcium.Value / ChemicalConstants.CalciumWeight;

        if (excludeProperty != nameof(WaterSample.Magnesium) && sample.Magnesium.HasValue)
            sum += sample.Magnesium.Value / ChemicalConstants.MagnesiumWeight;

        if (excludeProperty != nameof(WaterSample.Sodium) && sample.Sodium.HasValue)
            sum += sample.Sodium.Value / ChemicalConstants.SodiumWeight;

        if (excludeProperty != nameof(WaterSample.Potassium) && sample.Potassium.HasValue)
            sum += sample.Potassium.Value / ChemicalConstants.PotassiumWeight;

        return sum;
    }

    private static double CalculateAnionsSumWithout(WaterSample sample, string excludeProperty)
    {
        double sum = 0;

        if (excludeProperty != nameof(WaterSample.TotalAlkalinity) && sample.TotalAlkalinity.HasValue)
            sum += sample.TotalAlkalinity.Value / ChemicalConstants.AlkalinityWeight;

        if (excludeProperty != nameof(WaterSample.Chloride) && sample.Chloride.HasValue)
            sum += sample.Chloride.Value / ChemicalConstants.ChlorideWeight;

        if (excludeProperty != nameof(WaterSample.Fluoride) && sample.Fluoride.HasValue)
            sum += sample.Fluoride.Value / ChemicalConstants.FluorideWeight;

        if (excludeProperty != nameof(WaterSample.Nitrate) && sample.Nitrate.HasValue)
            sum += sample.Nitrate.Value / ChemicalConstants.NitrateWeight;

        if (excludeProperty != nameof(WaterSample.Sulfate) && sample.Sulfate.HasValue)
            sum += sample.Sulfate.Value / ChemicalConstants.SulfateWeight;

        return sum;
    }

    private static double GetMolarMass(string? propertyName)
    {
        return propertyName switch
        {
            nameof(WaterSample.Calcium) => ChemicalConstants.CalciumWeight,
            nameof(WaterSample.Magnesium) => ChemicalConstants.MagnesiumWeight,
            nameof(WaterSample.Sodium) => ChemicalConstants.SodiumWeight,
            nameof(WaterSample.Potassium) => ChemicalConstants.PotassiumWeight,
            nameof(WaterSample.Chloride) => ChemicalConstants.ChlorideWeight,
            nameof(WaterSample.Fluoride) => ChemicalConstants.FluorideWeight,
            nameof(WaterSample.Nitrate) => ChemicalConstants.NitrateWeight,
            nameof(WaterSample.Sulfate) => ChemicalConstants.SulfateWeight,
            nameof(WaterSample.TotalAlkalinity) => ChemicalConstants.AlkalinityWeight,
            _ => throw new ArgumentException($"Unknown property: {propertyName}")
        };
    }
}
