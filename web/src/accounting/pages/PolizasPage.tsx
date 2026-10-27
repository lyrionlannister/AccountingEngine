import { ArrowLeft, BookText } from 'lucide-react'
import { useNavigate } from 'react-router'
import { Breadcrumb } from '../../ui/components/Breadcrumb'
import { Button } from '../../ui/components/Button'
import { EmptyState } from '../../ui/components/EmptyState'

export const PolizasPage = () => {
  const navigate = useNavigate()

  return (
    <div className="grid gap-5">
      <div>
        <Breadcrumb items={[{ label: 'Contabilidad' }, { label: 'Pólizas' }]} />
        <h1 className="mt-2 font-head text-[22px] font-semibold text-text">Pólizas</h1>
        <p className="mt-1 text-[12.5px] text-muted">Libro diario: pólizas de ingreso, egreso y diario</p>
      </div>
      <EmptyState
        icon={<BookText className="size-5" aria-hidden />}
        title="Pantalla pendiente de diseño"
        description="Contenido previsto para el módulo de pólizas:"
        items={[
          'Registro de pólizas de ingreso, egreso y diario',
          'Validación de cuadre debe = haber antes de contabilizar',
          'Búsqueda, filtro por tipo/estado y orden por columna',
          'Reversa de pólizas canceladas con póliza diario automática',
        ]}
        actions={
          <Button onClick={() => navigate('/panel')}>
            <ArrowLeft className="size-3.5" aria-hidden />
            Volver al panel
          </Button>
        }
      />
    </div>
  )
}
