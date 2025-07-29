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

        var unknownProperty = WaterSampleValidator.GetUnknownProperty(sample);
        if (unknownProperty == null)
        {
            return new BalanceResult(
                ErrorMessage: "Unable to determine unknown property",
                Status: "Calculation Error"
            );
        }

        try
        {
            if (unknownProperty == nameof(WaterSample.Conductivity))
            {
                return CalculateConductivity(sample);
            }

            return CalculateIon(sample, unknownProperty);
        }
        catch (Exception ex)
        {
            return new BalanceResult(
                ErrorMessage: $"Calculation error: {ex.Message}",
                Status: "Calculation Error"
            );
        }
    }

    private static BalanceResult CalculateIon(WaterSample sample, string propertyName)
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
                throw new ArgumentException($"Unknown property type: {propertyName}");
        }

        var value = equivalentSum * weight;
        if (value < 0)
        {
            return new BalanceResult(
                ErrorMessage: $"Calculated concentration for {propertyName} is negative. Check input values.",
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
            Status: "Calculation Complete",
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
        return (sample.Calcium!.Value / ChemicalConstants.CalciumWeight) +
               (sample.Magnesium!.Value / ChemicalConstants.MagnesiumWeight) +
               (sample.Sodium!.Value / ChemicalConstants.SodiumWeight) +
               (sample.Potassium!.Value / ChemicalConstants.PotassiumWeight);
    }

    private static double CalculateAnionsSum(WaterSample sample)
    {
        return (sample.TotalAlkalinity!.Value / ChemicalConstants.AlkalinityWeight) +
               (sample.Chloride!.Value / ChemicalConstants.ChlorideWeight) +
               (sample.Fluoride!.Value / ChemicalConstants.FluorideWeight) +
               (sample.Nitrate!.Value / ChemicalConstants.NitrateWeight) +
               (sample.Sulfate!.Value / ChemicalConstants.SulfateWeight);
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
}
