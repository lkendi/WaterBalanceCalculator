using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using WaterBalanceCalculator.Models;
using WaterBalanceCalculator.Services;
namespace WaterBalanceCalculator;

public partial class MainWindow : Window
{
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
        DisplayResults(result);
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
        SolvedPropertyText.Text = "";
        SolvedValueText.Text = "";
        CationsSumText.Text = "";
        AnionsSumText.Text = "";

        StatusBorder.Background = Brush.Parse("#fef2f2");
        StatusBorder.BorderBrush = Brush.Parse("#fecaca");
        StatusIndicator.Background = Brush.Parse("#ef4444");
        StatusText.Foreground = Brush.Parse("#b91c1c");
    }

    private void DisplayResults(BalanceResult result)
    {
        StatusText.Text = result.Status;
        SolvedPropertyText.Text = result.SolvedProperty != null ? FormatPropertyName(result.SolvedProperty) : "";
        SolvedValueText.Text = result.SolvedValue.HasValue ? $"{result.SolvedValue:F2}" : "";
        CationsSumText.Text = result.CationsSum.HasValue ? $"{result.CationsSum:F3}" : "";
        AnionsSumText.Text = result.AnionsSum.HasValue ? $"{result.AnionsSum:F3}" : "";
        ErrorMessageText.Text = result.ErrorMessage ?? "";
    }

    private string FormatPropertyName(string propertyName)
    {
        return propertyName switch
        {
            "TotalAlkalinity" => "Total Alkalinity",
            _ => propertyName
        };
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
        SolvedPropertyText.Text = "";
        SolvedValueText.Text = "";
        CationsSumText.Text = "";
        AnionsSumText.Text = "";
        ErrorMessageText.Text = "";


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