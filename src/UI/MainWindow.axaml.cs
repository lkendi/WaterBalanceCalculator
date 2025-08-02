using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WaterBalanceCalculator.Models;
using WaterBalanceCalculator.Services;
using WaterBalanceCalculator.Constants;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace WaterBalanceCalculator;

public partial class MainWindow : Window
{
    private readonly Dictionary<string, int> propertyToRowIndex = new()
    {
        { nameof(WaterSample.Calcium), 1 },
        { nameof(WaterSample.Magnesium), 2 },
        { nameof(WaterSample.Sodium), 3 },
        { nameof(WaterSample.Potassium), 4 },
        { nameof(WaterSample.Chloride), 5 },
        { nameof(WaterSample.Fluoride), 6 },
        { nameof(WaterSample.Nitrate), 7 },
        { nameof(WaterSample.Sulfate), 8 },
        { nameof(WaterSample.TotalAlkalinity), 9 },
        { nameof(WaterSample.Conductivity), 10 }
    };

    public MainWindow()
    {
        InitializeComponent();
        CalculateButton.Click += OnCalculateClicked;
        ClearButton.Click += OnClearClicked;

        WindowState = WindowState.Maximized;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    private void OnCalculateClicked(object? sender, RoutedEventArgs e)
    {
        var sample = ParseInputs();

        var validation = WaterSampleValidator.ValidateForCalculation(sample);
        if (!validation.IsValid)
        {
            DisplayError(validation.Message);
            return;
        }

        var result = WaterBalanceCalculatorService.Calculate(sample);

        if (result.SolvedProperty != null && result.SolvedValue.HasValue)
        {
            var property = typeof(WaterSample).GetProperty(result.SolvedProperty);
            property?.SetValue(sample, result.SolvedValue);
        }

        DisplayResults(sample, result);
        UpdateStatusUI(result);
    }

    private WaterSample ParseInputs()
    {
        double? ParseBox(TextBox box) =>
            string.IsNullOrWhiteSpace(box.Text) ? null :
            double.TryParse(box.Text, out var val) ? val : null;

        return new WaterSample
        {
            Calcium = ParseBox(CalciumBox),
            Magnesium = ParseBox(MagnesiumBox),
            Sodium = ParseBox(SodiumBox),
            Potassium = ParseBox(PotassiumBox),
            Chloride = ParseBox(ChlorideBox),
            Fluoride = ParseBox(FluorideBox),
            Nitrate = ParseBox(NitrateBox),
            Sulfate = ParseBox(SulfateBox),
            TotalAlkalinity = ParseBox(AlkalinityBox),
            Conductivity = ParseBox(ConductivityBox)
        };
    }

    private void DisplayError(string message)
    {
        StatusText.Text = "Input Error";
        ErrorMessageText.Text = message;
        ClearResultsTable();
        StatusBorder.Background = Brush.Parse("#fef2f2");
        StatusBorder.BorderBrush = Brush.Parse("#fecaca");
        StatusIndicator.Background = Brush.Parse("#ef4444");
        StatusText.Foreground = Brush.Parse("#b91c1c");
    }

    private void ClearResultsTable()
    {
        CalciumValueText.Text = "";
        CalciumMolesText.Text = "";
        MagnesiumValueText.Text = "";
        MagnesiumMolesText.Text = "";
        SodiumValueText.Text = "";
        SodiumMolesText.Text = "";
        PotassiumValueText.Text = "";
        PotassiumMolesText.Text = "";
        ChlorideValueText.Text = "";
        ChlorideMolesText.Text = "";
        FluorideValueText.Text = "";
        FluorideMolesText.Text = "";
        NitrateValueText.Text = "";
        NitrateMolesText.Text = "";
        SulfateValueText.Text = "";
        SulfateMolesText.Text = "";
        AlkalinityValueText.Text = "";
        AlkalinityMolesText.Text = "";
        ConductivityValueText.Text = "";
        ConductivityMolesText.Text = "";
        CationsSumValueText.Text = "";
        CationsSumMolesText.Text = "";
        AnionsSumValueText.Text = "";
        AnionsSumMolesText.Text = "";

        HighlightSolvedParameter(null);
    }

    private void DisplayResults(WaterSample sample, BalanceResult result)
    {
        UpdateParameterRow(sample.Calcium, CalciumValueText, CalciumMolesText, ChemicalConstants.CalciumWeight);
        UpdateParameterRow(sample.Magnesium, MagnesiumValueText, MagnesiumMolesText, ChemicalConstants.MagnesiumWeight);
        UpdateParameterRow(sample.Sodium, SodiumValueText, SodiumMolesText, ChemicalConstants.SodiumWeight);
        UpdateParameterRow(sample.Potassium, PotassiumValueText, PotassiumMolesText, ChemicalConstants.PotassiumWeight);
        UpdateParameterRow(sample.Chloride, ChlorideValueText, ChlorideMolesText, ChemicalConstants.ChlorideWeight);
        UpdateParameterRow(sample.Fluoride, FluorideValueText, FluorideMolesText, ChemicalConstants.FluorideWeight);
        UpdateParameterRow(sample.Nitrate, NitrateValueText, NitrateMolesText, ChemicalConstants.NitrateWeight);
        UpdateParameterRow(sample.Sulfate, SulfateValueText, SulfateMolesText, ChemicalConstants.SulfateWeight);
        UpdateParameterRow(sample.TotalAlkalinity, AlkalinityValueText, AlkalinityMolesText, ChemicalConstants.AlkalinityWeight);
        UpdateParameterRow(sample.Conductivity, ConductivityValueText, ConductivityMolesText, null);

        HighlightSolvedParameter(result.SolvedProperty);

        if (result.CationsSum.HasValue)
        {
            CationsSumValueText.Text = $"{result.CationsSum:F3}";
            CationsSumMolesText.Text = $"--";
        }
        else
        {
            CationsSumValueText.Text = "";
            CationsSumMolesText.Text = "";
        }

        if (result.AnionsSum.HasValue)
        {
            AnionsSumValueText.Text = $"{result.AnionsSum:F3}";
            AnionsSumMolesText.Text = $"--";
        }
        else
        {
            AnionsSumValueText.Text = "";
            AnionsSumMolesText.Text = "";
        }

        StatusText.Text = result.Status;
        ErrorMessageText.Text = result.ErrorMessage ?? "";
    }

    private void UpdateParameterRow(double? value, TextBlock valueText, TextBlock molesText, double? molarMass)
    {
        if (value.HasValue)
        {
            valueText.Text = $"{value:F2}";
            if (molarMass.HasValue)
            {
                molesText.Text = $"{(value.Value / molarMass.Value):F3}";
            }
            else
            {
                molesText.Text = "--";
            }
        }
        else
        {
            valueText.Text = "";
            molesText.Text = "";
        }
    }

    private void HighlightSolvedParameter(string? solvedProperty)
    {
        for (int row = 1; row <= 10; row++)
        {
            SetRowBold(row, false);
        }

        if (solvedProperty != null && propertyToRowIndex.TryGetValue(solvedProperty, out int rowIndex))
        {
            SetRowBold(rowIndex, true);
        }
    }

    private void SetRowBold(int row, bool isBold)
    {
        foreach (var child in ResultsTable.Children)
        {
            if (Grid.GetRow(child) == row && child is TextBlock textBlock)
            {
                if (Grid.GetColumn(child) == 2)
                {
                    textBlock.FontWeight = FontWeight.Normal;
                }
                else
                {
                    textBlock.FontWeight = isBold ? FontWeight.Bold : FontWeight.Normal;
                }
            }
        }
    }

    private void OnClearClicked(object? sender, RoutedEventArgs e)
    {
        CalciumBox.Text = "";
        MagnesiumBox.Text = "";
        SodiumBox.Text = "";
        PotassiumBox.Text = "";
        ChlorideBox.Text = "";
        FluorideBox.Text = "";
        NitrateBox.Text = "";
        SulfateBox.Text = "";
        AlkalinityBox.Text = "";
        ConductivityBox.Text = "";

        StatusText.Text = "Enter values and calculate";
        ErrorMessageText.Text = "";
        ClearResultsTable();

        StatusBorder.Background = Brush.Parse("#eff6ff");
        StatusBorder.BorderBrush = Brush.Parse("#dbeafe");
        StatusIndicator.Background = Brush.Parse("#3b82f6");
        StatusText.Foreground = Brush.Parse("#1e40af");
    }

    private void UpdateStatusUI(BalanceResult result)
    {
        if (result.ErrorMessage != null)
        {
            StatusBorder.Background = Brush.Parse("#fef2f2");
            StatusBorder.BorderBrush = Brush.Parse("#fecaca");
            StatusIndicator.Background = Brush.Parse("#ef4444");
            StatusText.Foreground = Brush.Parse("#b91c1c");
        }
        else if (result.Status == "Calculation Complete")
        {
            StatusBorder.Background = Brush.Parse("#ecfdf5");
            StatusBorder.BorderBrush = Brush.Parse("#bbf7d0");
            StatusIndicator.Background = Brush.Parse("#16a34a");
            StatusText.Foreground = Brush.Parse("#166534");
        }
        else if (result.Status == "Invalid Result")
        {
            StatusBorder.Background = Brush.Parse("#fff7ed");
            StatusBorder.BorderBrush = Brush.Parse("#ffedd5");
            StatusIndicator.Background = Brush.Parse("#f97316");
            StatusText.Foreground = Brush.Parse("#c2410c");
        }
        else
        {
            StatusBorder.Background = Brush.Parse("#fef2f2");
            StatusBorder.BorderBrush = Brush.Parse("#fecaca");
            StatusIndicator.Background = Brush.Parse("#ef4444");
            StatusText.Foreground = Brush.Parse("#b91c1c");
        }
    }
}
