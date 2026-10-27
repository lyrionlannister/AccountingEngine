using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia;
using AccountingEngine.Desktop.Shared;

namespace AccountingEngine.Desktop.Accounting.ViewModels;

public sealed record StatCardItem(string Label, string Code, string Value, string Trend);

public sealed record ChartGridLine(double Y, string Label);

public sealed record ChartMonthLabel(double X, string Label);

public partial class PanelViewModel : ViewModelBase
{
    private const double ChartLeft = 46, ChartRight = 556, ChartTop = 10, ChartBottom = 150;
    private const double MinValue = 2000, MaxValue = 4200;

    private static readonly double[] Ingresos = [2980, 3340, 3650, 3910];
    private static readonly double[] Gastos = [2410, 2560, 2690, 2840];
    private static readonly string[] Meses = ["Jul", "Ago", "Sep", "Oct"];

    public ObservableCollection<StatCardItem> Stats { get; } =
    [
        new("Ingresos del mes", "4000", "$3,910,400.00", "+7.1%"),
        new("Gastos del mes", "6000", "$2,840,150.00", "+2.9%"),
        new("Utilidad de operación", "Ing − Gto", "$1,070,250.00", "+20.3%"),
    ];

    public ObservableCollection<MovimientoRow> Movimientos { get; } =
    [
        new("2026-10-27", "PE-2026-0878", "Arrendamiento de oficinas", MovimientoEstado.Conciliado, -64000m),
        new("2026-10-26", "PI-2026-1142", "Factura B-5521 · Inmobiliaria Reforma", MovimientoEstado.Conciliado, 182400m),
        new("2026-10-24", "PE-2026-0876", "Nómina quincenal", MovimientoEstado.Pendiente, -241300m),
        new("2026-10-20", "PD-2026-0520", "Reversa póliza cancelada", MovimientoEstado.Vencido, 64000m),
    ];

    public IReadOnlyList<Point> IngresosLine { get; } = ToLine(Ingresos);
    public IReadOnlyList<Point> GastosLine { get; } = ToLine(Gastos);
    public IReadOnlyList<Point> IngresosArea { get; } = ToArea(Ingresos);
    public IReadOnlyList<Point> GastosArea { get; } = ToArea(Gastos);

    public Point IngresosEnd => IngresosLine[^1];
    public Point GastosEnd => GastosLine[^1];

    public IReadOnlyList<ChartGridLine> GridLines { get; } =
    [
        ToGridLine(4000), ToGridLine(3000), ToGridLine(2000),
    ];

    public IReadOnlyList<ChartMonthLabel> MonthLabels { get; } = Meses
        .Select((m, i) => new ChartMonthLabel(XFor(i), m))
        .ToList();

    private static double XFor(int index) => ChartLeft + index * (ChartRight - ChartLeft) / (Ingresos.Length - 1);

    private static double YFor(double value) => ChartTop + (MaxValue - value) / (MaxValue - MinValue) * (ChartBottom - ChartTop);

    private static ChartGridLine ToGridLine(double value) => new(YFor(value), $"${value / 1000:0.0} M");

    private static IReadOnlyList<Point> ToLine(double[] values) =>
        values.Select((v, i) => new Point(XFor(i), YFor(v))).ToList();

    private static IReadOnlyList<Point> ToArea(double[] values)
    {
        var line = ToLine(values);
        return [.. line, new Point(XFor(values.Length - 1), ChartBottom), new Point(XFor(0), ChartBottom)];
    }
}
