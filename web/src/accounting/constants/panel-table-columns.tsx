import { Badge } from '../../ui/components/Badge'
import type { TableColumn } from '../../ui/types/table'
import type { Movimiento } from '../types/movimiento'
import { AMOUNT_FORMATTER_MXN } from './formatters'

export const PANEL_MOVIMIENTO_STATUS_LABEL: Record<Movimiento['estado'], string> = {
  success: 'Conciliado',
  warning: 'Pendiente',
  error: 'Vencido',
}

export const PANEL_TABLE_COLUMNS: TableColumn<Movimiento>[] = [
  { key: 'fecha', header: 'Fecha', sortable: true, render: (r) => <span className="font-mono text-muted">{r.fecha}</span> },
  { key: 'poliza', header: 'Póliza', sortable: true, render: (r) => <span className="font-mono text-link">{r.poliza}</span> },
  { key: 'concepto', header: 'Concepto', render: (r) => <>{r.concepto}</> },
  { key: 'estado', header: 'Estado', render: (r) => <Badge tone={r.estado}>{PANEL_MOVIMIENTO_STATUS_LABEL[r.estado]}</Badge> },
  {
    key: 'importe',
    header: 'Importe MXN',
    align: 'right',
    sortable: true,
    render: (r) => <span className="font-medium tabular-nums">{AMOUNT_FORMATTER_MXN.format(r.importe)}</span>,
  },
]
