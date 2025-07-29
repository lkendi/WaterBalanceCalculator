using Xunit;
using WaterBalanceCalculator.Models;
using WaterBalanceCalculator.Services;

namespace WaterBalanceCalculator.Tests;

public class WaterBalanceCalculatorServiceUnitTests
{
    [Fact]
    public void Calculate_ReturnsError_WhenInvalidInput()
    {
        var sample = new WaterSample();
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Invalid Input", result.Status);
        Assert.Contains("Multiple unknown values", result.ErrorMessage);
    }

    [Fact]
    public void Calculate_ReturnsError_WhenUnknownPropertyCannotBeDetermined()
    {
        var sample = new WaterSample
        {
            Calcium = 10, Magnesium = 10, Sodium = 10, Potassium = 10,
            Chloride = 10, Fluoride = 10, Nitrate = 10, Sulfate = 10,
            TotalAlkalinity = 10, Conductivity = 10
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Invalid Input", result.Status);
        Assert.Contains("All values are provided", result.ErrorMessage);
    }

    [Fact]
    public void Calculate_ReturnsError_WhenNegativeValue()
    {
        var sample = new WaterSample
        {
            Calcium = -1, Magnesium = 10, Sodium = 10, Potassium = 10,
            Chloride = 10, Fluoride = 10, Nitrate = 10, Sulfate = 10,
            TotalAlkalinity = 10, Conductivity = 10
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Invalid Input", result.Status);
        Assert.Contains("Negative values not allowed", result.ErrorMessage);
    }

    [Fact]
    public void Calculate_CalculatesCalcium_WhenCalciumIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = null, Magnesium = 12, Sodium = 23, Potassium = 39,
            Chloride = 35.5, Fluoride = 19, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Calcium), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
        Assert.True(result.CationsSum >= 0);
        Assert.True(result.AnionsSum >= 0);
    }

    [Fact]
    public void Calculate_CalculatesMagnesium_WhenMagnesiumIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = null, Sodium = 23, Potassium = 39,
            Chloride = 35.5, Fluoride = 19, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Magnesium), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesSodium_WhenSodiumIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = null, Potassium = 39,
            Chloride = 35.5, Fluoride = 19, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Sodium), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesPotassium_WhenPotassiumIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = 23, Potassium = null,
            Chloride = 35.5, Fluoride = 19, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Potassium), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesChloride_WhenChlorideIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = 23, Potassium = 39,
            Chloride = null, Fluoride = 19, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Chloride), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesFluoride_WhenFluorideIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = 23, Potassium = 39,
            Chloride = 35.5, Fluoride = null, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Fluoride), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesNitrate_WhenNitrateIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = 23, Potassium = 39,
            Chloride = 35.5, Fluoride = 19, Nitrate = null, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Nitrate), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesSulfate_WhenSulfateIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = 23, Potassium = 39,
            Chloride = 35.5, Fluoride = 19, Nitrate = 14, Sulfate = null,
            TotalAlkalinity = 50, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.Sulfate), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesTotalAlkalinity_WhenTotalAlkalinityIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = 23, Potassium = 39,
            Chloride = 35.5, Fluoride = 19, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = null, Conductivity = 250
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal(nameof(WaterSample.TotalAlkalinity), result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }

    [Fact]
    public void Calculate_CalculatesConductivity_WhenConductivityIsUnknown()
    {
        var sample = new WaterSample
        {
            Calcium = 20, Magnesium = 12, Sodium = 23, Potassium = 39,
            Chloride = 35.5, Fluoride = 19, Nitrate = 14, Sulfate = 48,
            TotalAlkalinity = 50, Conductivity = null
        };
        var result = WaterBalanceCalculatorService.Calculate(sample);
        Assert.Equal("Calculation Complete", result.Status);
        Assert.Equal("Conductivity", result.SolvedProperty);
        Assert.True(result.SolvedValue >= 0);
    }
}
