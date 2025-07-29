using Xunit;
using WaterBalanceCalculator.Models;
using WaterBalanceCalculator.Services;

namespace WaterBalanceCalculator.Tests;

public class WaterSampleValidatorUnitTests
{
    [Fact]
    public void AllValuesPresent_ReturnsError()
    {
        var sample = new WaterSample
        {
            Calcium = 10, Magnesium = 5, Sodium = 8, Potassium = 2,
            Chloride = 7, Fluoride = 1, Nitrate = 3, Sulfate = 4,
            TotalAlkalinity = 20, Conductivity = 100
        };
        var result = WaterSampleValidator.ValidateForCalculation(sample);
        Assert.False(result.IsValid);
        Assert.Contains("At least one value must be unknown", result.Message);
    }

    [Fact]
    public void MultipleUnknowns_ReturnsError()
    {
        var sample = new WaterSample
        {
            Calcium = null, Magnesium = null, Sodium = 8, Potassium = 2,
            Chloride = 7, Fluoride = 1, Nitrate = 3, Sulfate = 4,
            TotalAlkalinity = 20, Conductivity = 100
        };
        var result = WaterSampleValidator.ValidateForCalculation(sample);
        Assert.False(result.IsValid);
        Assert.Contains("Multiple unknown values", result.Message);
    }

     [Fact]
    public void AllUnknowns_ReturnsError()
    {
        var sample = new WaterSample();
        var result = WaterSampleValidator.ValidateForCalculation(sample);
        Assert.False(result.IsValid);
        Assert.Contains("Multiple unknown values", result.Message);
    }

    [Fact]
    public void OneUnknown_ReturnsValid()
    {
        var sample = new WaterSample
        {
            Calcium = null, Magnesium = 5, Sodium = 8, Potassium = 2,
            Chloride = 7, Fluoride = 1, Nitrate = 3, Sulfate = 4,
            TotalAlkalinity = 20, Conductivity = 100
        };
        var result = WaterSampleValidator.ValidateForCalculation(sample);
        Assert.True(result.IsValid);
        Assert.Contains("Valid for calculation", result.Message);
    }

    [Fact]
    public void NegativeValue_ReturnsError()
    {
        var sample = new WaterSample
        {
            Calcium = -1, Magnesium = 5, Sodium = 8, Potassium = 2,
            Chloride = 7, Fluoride = 1, Nitrate = 3, Sulfate = 4,
            TotalAlkalinity = 20, Conductivity = 100
        };
        var result = WaterSampleValidator.ValidateForCalculation(sample);
        Assert.False(result.IsValid);
        Assert.Contains("Negative values not allowed", result.Message);
    }

    [Fact]
    public void GetUnknownProperty_ReturnsCorrectProperty()
    {
        var sample = new WaterSample
        {
            Calcium = 10, Magnesium = 5, Sodium = 8, Potassium = 2,
            Chloride = 7, Fluoride = 1, Nitrate = 3, Sulfate = 4,
            TotalAlkalinity = null, Conductivity = 100
        };
        var property = WaterSampleValidator.GetUnknownProperty(sample);
        Assert.Equal(nameof(WaterSample.TotalAlkalinity), property);
    }

    [Fact]
    public void NullSample_ReturnsError()
    {
        var result = WaterSampleValidator.ValidateForCalculation(null);
        Assert.False(result.IsValid);
        Assert.Contains("cannot be null", result.Message);
    }
}
