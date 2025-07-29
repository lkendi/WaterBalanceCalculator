namespace WaterBalanceCalculator.Models;

/// <summary>
/// model that defines a water sample (input)
/// </summary>

public class WaterSample
{
    public double? Calcium { get; set; }
    public double? Magnesium { get; set; }
    public double? Sodium { get; set; }
    public double? Potassium { get; set; }
    public double? Chloride { get; set; }
    public double? Fluoride { get; set; }
    public double? Nitrate { get; set; }
    public double? Sulfate { get; set; }
    public double? TotalAlkalinity { get; set; }
    public double? Conductivity { get; set; }

}
