using System.Collections.ObjectModel;
using AccountingEngine.Desktop.Shared;

namespace AccountingEngine.Desktop.Accounting.ViewModels;

public partial class PolizasViewModel : ViewModelBase
{
    public ObservableCollection<string> Plan { get; } =
    [
        "Registro de pólizas de ingreso, egreso y diario",
        "Validación de cuadre debe = haber antes de contabilizar",
        "Búsqueda, filtro por tipo/estado y orden por columna",
        "Reversa de pólizas canceladas con póliza diario automática",
    ];
}
