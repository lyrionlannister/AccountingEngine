using System.Collections.ObjectModel;
using AccountingEngine.Desktop.Shared;

namespace AccountingEngine.Desktop.Accounting.ViewModels;

public partial class PanelViewModel : ViewModelBase
{
    public ObservableCollection<string> Plan { get; } =
    [
        "Indicadores de ingresos, gastos y utilidad del periodo",
        "Gráfica de ingresos vs. gastos por mes",
        "Tabla de transacciones recientes",
    ];
}
