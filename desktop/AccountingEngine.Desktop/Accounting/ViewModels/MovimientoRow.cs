namespace AccountingEngine.Desktop.Accounting.ViewModels;

public enum MovimientoEstado
{
    Conciliado,
    Pendiente,
    Vencido,
}

public sealed record MovimientoRow(string Fecha, string Poliza, string Concepto, MovimientoEstado Estado, decimal Importe)
{
    public string ImporteDisplay => Importe.ToString("C2", new System.Globalization.CultureInfo("es-MX"));

    public string EstadoLabel => Estado switch
    {
        MovimientoEstado.Conciliado => "Conciliado",
        MovimientoEstado.Pendiente => "Pendiente",
        MovimientoEstado.Vencido => "Vencido",
        _ => Estado.ToString(),
    };

    public string EstadoTone => Estado switch
    {
        MovimientoEstado.Conciliado => "success",
        MovimientoEstado.Pendiente => "warning",
        MovimientoEstado.Vencido => "error",
        _ => "neutral",
    };
}
