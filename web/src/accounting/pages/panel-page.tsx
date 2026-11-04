import { useMemo, useState } from 'react'
import Chart from 'react-apexcharts'
import { Download } from 'lucide-react'
import { Breadcrumb } from '../../ui/components/Breadcrumb'
import { Button } from '../../ui/components/Button'
import { Table } from '../../ui/components/Table'
import type { SortDirection } from '../../ui/types/table'
import { StatCard } from '../components/StatCard'
import { MOVIMIENTOS } from '../constants/movimientos'
import { PANEL_CHART_SERIES } from '../constants/panel-chart-series'
import { PANEL_TABLE_COLUMNS } from '../constants/panel-table-columns'
import type { Movimiento } from '../types/movimiento'

export const PanelPage = () => {
  const [sortKey, setSortKey] = useState<string>('fecha')
  const [sortDir, setSortDir] = useState<SortDirection>('desc')

  const rows = useMemo(() => {
    const sorted = [...MOVIMIENTOS].sort((a, b) => {
      const dir = sortDir === 'asc' ? 1 : -1
      if (sortKey === 'importe') return (a.importe - b.importe) * dir
      const av = a[sortKey as keyof Movimiento], bv = b[sortKey as keyof Movimiento]
      return String(av).localeCompare(String(bv)) * dir
    })
    return sorted
  }, [sortKey, sortDir])

  return (
    <div className="grid gap-5">
      <div className="flex items-end justify-between gap-4">
        <div>
          <Breadcrumb items={[{ label: 'General' }, { label: 'Panel' }]} />
          <h1 className="mt-2 font-head text-[22px] font-semibold text-text">Panel financiero</h1>
          <p className="mt-1 text-[12.5px] text-muted">Octubre 2026 · cifras en MXN</p>
        </div>
        <Button>
          <Download className="size-3.5" aria-hidden />
          Exportar balanza
        </Button>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
        <StatCard label="Ingresos del mes" code="4000" value="$3,910,400.00" trend="+7.1%" />
        <StatCard label="Gastos del mes" code="6000" value="$2,840,150.00" trend="+2.9%" />
        <StatCard label="Utilidad de operación" code="Ing − Gto" value="$1,070,250.00" trend="+20.3%" />
      </div>

      <div className="rounded-md border border-line bg-surface p-4.5 shadow-card">
        <h2 className="font-head text-[15px] font-semibold text-text">Ingresos vs. gastos</h2>
        <p className="text-xs text-muted">Mensual, miles de MXN</p>
        <Chart
          type="area"
          height={260}
          series={PANEL_CHART_SERIES}
          options={{
            chart: { toolbar: { show: false } },
            colors: ['#2e64b5', '#12a0b0'],
            dataLabels: { enabled: false },
            stroke: { width: 2, curve: 'smooth' },
            xaxis: { categories: ['Jul', 'Ago', 'Sep', 'Oct'] },
            legend: { position: 'top', horizontalAlign: 'left' },
          }}
        />
      </div>

      <div className="rounded-md border border-line bg-surface shadow-card">
        <div className="flex items-center justify-between px-4.5 py-3.5">
          <div>
            <h2 className="font-head text-[15px] font-semibold text-text">Transacciones recientes</h2>
            <p className="text-xs text-muted">Pólizas de ingreso, egreso y diario · octubre 2026</p>
          </div>
        </div>
        <Table
          columns={PANEL_TABLE_COLUMNS}
          rows={rows}
          rowKey={(r) => r.id}
          sortKey={sortKey}
          sortDir={sortDir}
          onSort={(key) => {
            if (key === sortKey) setSortDir(sortDir === 'asc' ? 'desc' : 'asc')
            else {
              setSortKey(key)
              setSortDir(key === 'fecha' || key === 'importe' ? 'desc' : 'asc')
            }
          }}
        />
      </div>
    </div>
  )
}
