import { ArrowLeft, LayoutGrid } from 'lucide-react'
import { useNavigate } from 'react-router'
import { Breadcrumb } from '../../ui/components/breadcrumb'
import { Button } from '../../ui/components/button'
import { EmptyState } from '../../ui/components/empty-state'

export const PanelPage = () => {
  const navigate = useNavigate()

  return (
    <div className="grid gap-5">
      <div>
        <Breadcrumb items={[{ label: 'General' }, { label: 'Panel' }]} />
        <h1 className="mt-2 font-head text-[22px] font-semibold text-text">Panel financiero</h1>
        <p className="mt-1 text-[12.5px] text-muted">Resumen de ingresos, gastos y pólizas recientes</p>
      </div>
      <EmptyState
        icon={<LayoutGrid className="size-5" aria-hidden />}
        title="Pantalla pendiente de diseño"
        description="Contenido previsto para el panel financiero:"
        items={[
          'Indicadores de ingresos, gastos y utilidad del periodo',
          'Gráfica de ingresos vs. gastos por mes',
          'Tabla de transacciones recientes',
        ]}
        actions={
          <Button onClick={() => navigate('/polizas')}>
            <ArrowLeft className="size-3.5" aria-hidden />
            Ir a pólizas
          </Button>
        }
      />
    </div>
  )
}
