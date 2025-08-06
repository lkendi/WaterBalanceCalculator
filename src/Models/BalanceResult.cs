namespace WaterBalanceCalculator.Models;

/// <summary>
/// model that defines the result after calculation (output)
/// </summary>

public record BalanceResult(
    double? CationsSum = null,
    double? AnionsSum = null,
    string? Status = null,
    string? SolvedProperty = null,
    double? SolvedValue = null,
    string? SecondSolvedProperty = null,
    double? SecondSolvedValue = null,
    string? ErrorMessage = null
);
