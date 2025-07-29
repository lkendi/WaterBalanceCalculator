namespace WaterBalanceCalculator.Models;

/// <summary>
/// model that defines the result after calculation (output)
/// </summary>
public record BalanceResult(
    double? CationsSum = null,
    double? AnionsSum = null,
    double? ErrorPercentage = null,
    string? Status = null,
    string? SolvedProperty = null,
    double? SolvedValue = null,
    string? ErrorMessage = null
);
