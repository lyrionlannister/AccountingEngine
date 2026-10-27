import { useState } from 'react'
import { Copy, Download, Eye, FileText, Plus, Trash2 } from 'lucide-react'
import { Card } from '../components/Card'
import { Breadcrumb } from '../../ui/components/Breadcrumb'
import { Button } from '../../ui/components/Button'
import { Badge } from '../../ui/components/Badge'
import { Alert } from '../../ui/components/Alert'
import { Field } from '../../ui/components/Field'
import { Input } from '../../ui/components/Input'
import { Select } from '../../ui/components/Select'
import { Textarea } from '../../ui/components/Textarea'
import { Affix } from '../../ui/components/Affix'
import { Checkbox } from '../../ui/components/Checkbox'
import { Switch } from '../../ui/components/Switch'
import { SegmentedControl } from '../../ui/components/SegmentedControl'
import { Tabs } from '../../ui/components/Tabs'
import { Pagination } from '../../ui/components/Pagination'
import { DropdownMenu } from '../../ui/components/DropdownMenu'
import { Tooltip } from '../../ui/components/Tooltip'
import { Dialog } from '../../ui/components/Dialog'

export const ComponentesPage = () => {
  const [view, setView] = useState('mensual')
  const [tab, setTab] = useState('partidas')
  const [page, setPage] = useState(1)
  const [dialogOpen, setDialogOpen] = useState(false)

  return (
    <div className="grid gap-5">
      <div>
        <Breadcrumb items={[{ label: 'Sistema' }, { label: 'Componentes' }]} />
        <h1 className="mt-2 font-head text-[22px] font-semibold text-text">Biblioteca de componentes</h1>
        <p className="mt-1 text-[12.5px] text-muted">Botones, campos, navegación y retroalimentación con los tokens DIR-D</p>
      </div>

      <Card title="Botones">
        <div className="flex flex-wrap items-center gap-2.5">
          <Button variant="primary">
            <Plus className="size-3.5" aria-hidden />
            Primario
          </Button>
          <Button variant="secondary">Secundario</Button>
          <Button variant="ghost">Terciario</Button>
          <Button variant="danger">
            <Trash2 className="size-3.5" aria-hidden />
            Cancelar póliza
          </Button>
          <Button variant="danger-secondary">Peligro secundario</Button>
          <Button variant="link">Ver auxiliar</Button>
        </div>
        <div className="flex flex-wrap items-center gap-2.5">
          <Button variant="primary" size="sm">
            Pequeño
          </Button>
          <Button variant="primary">Mediano</Button>
          <Button variant="primary" size="lg">
            Grande
          </Button>
          <Button variant="primary" loading>
            Timbrando…
          </Button>
          <Button variant="secondary" disabled>
            Deshabilitado
          </Button>
          <Tooltip label="Duplicar póliza · ⌘D">
            <Button variant="secondary" iconOnly aria-label="Duplicar póliza">
              <Copy className="size-3.5" aria-hidden />
            </Button>
          </Tooltip>
        </div>
      </Card>

      <Card title="Campos de entrada" description="Etiqueta siempre visible · importes alineados a la derecha">
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          <Field id="rs" label="Razón social" help="Tal como aparece en el registro fiscal">
            <Input id="rs" defaultValue="Grupo Altamira S.A. de C.V." />
          </Field>
          <Field id="cta" label="Cuenta contable" tone="error" help="La cuenta 1105-9 no existe en el catálogo">
            <Input id="cta" mono invalid defaultValue="1105-9" />
          </Field>
          <Field id="imp" label="Importe">
            <Affix id="imp" defaultValue="248,600.00" suffix="MXN" prefix="$" />
          </Field>
          <Field id="metodo" label="Método de pago">
            <Select id="metodo" defaultValue="PPD">
              <option value="PUE">PUE · Pago en una sola exhibición</option>
              <option value="PPD">PPD · Pago en parcialidades o diferido</option>
            </Select>
          </Field>
          <Field id="fecha" label="Fecha de pago">
            <Input id="fecha" type="date" defaultValue="2026-10-27" />
          </Field>
          <Field id="nota" label="Notas internas" optional>
            <Textarea id="nota" placeholder="Visible solo para el despacho" />
          </Field>
        </div>
      </Card>

      <div className="grid grid-cols-1 gap-5 lg:grid-cols-2">
        <Card title="Selección">
          <div className="grid gap-2">
            <Checkbox defaultChecked>Incluir cuentas de orden</Checkbox>
            <Checkbox>Mostrar cuentas con saldo cero</Checkbox>
          </div>
          <div className="grid gap-2.5">
            <Switch defaultChecked>Contabilizar al timbrar el CFDI</Switch>
            <Switch>Aviso de cierre por correo</Switch>
          </div>
          <SegmentedControl
            aria-label="Vista"
            value={view}
            onChange={setView}
            options={[
              { value: 'mensual', label: 'Mensual' },
              { value: 'trimestral', label: 'Trimestral' },
              { value: 'anual', label: 'Anual' },
            ]}
          />
        </Card>

        <Card title="Navegación">
          <Tabs
            value={tab}
            onChange={setTab}
            tabs={[
              { key: 'resumen', label: 'Resumen' },
              { key: 'partidas', label: 'Partidas', count: 3 },
              { key: 'cfdi', label: 'CFDI adjuntos', count: 2 },
              { key: 'historial', label: 'Historial' },
            ]}
          />
          <Pagination page={page} pageCount={9} totalLabel="1–25 de 214 movimientos" onChange={setPage} />
          <DropdownMenu
            aria-label="Acciones de póliza"
            trigger={<Button variant="secondary">Acciones de póliza</Button>}
            items={[
              { label: 'Ver auxiliar', icon: <Eye className="size-3.5" aria-hidden />, kbd: 'A' },
              { label: 'Duplicar póliza', icon: <Copy className="size-3.5" aria-hidden />, kbd: '⌘D' },
              { label: 'Descargar XML', icon: <Download className="size-3.5" aria-hidden /> },
              'divider',
              { label: 'Cancelar póliza', icon: <Trash2 className="size-3.5" aria-hidden />, danger: true },
            ]}
          />
        </Card>
      </div>

      <Card title="Retroalimentación" description="Siempre con icono y texto, nunca solo color">
        <Alert tone="info" title="Septiembre está cerrado.">
          Los ajustes requieren reabrir el periodo.
        </Alert>
        <Alert tone="success" title="Balanza cuadrada.">
          Cargos y abonos suman $48,912,330.50.
        </Alert>
        <Alert tone="warning" title="14 movimientos sin conciliar" action={<Button size="sm">Conciliar</Button>}>
          en BBVA ****4821.
        </Alert>
        <Alert tone="error" title="El SAT rechazó la balanza XML.">
          El código agrupador 105.01 no coincide con la naturaleza de la cuenta 1105-01.
        </Alert>
        <div className="flex flex-wrap gap-2">
          <Badge tone="success">Conciliado</Badge>
          <Badge tone="warning">Pendiente</Badge>
          <Badge tone="error">Vencido</Badge>
          <Badge tone="neutral">Borrador</Badge>
        </div>
        <Button variant="secondary" onClick={() => setDialogOpen(true)}>
          <FileText className="size-3.5" aria-hidden />
          Abrir modal de póliza
        </Button>
      </Card>

      <Dialog
        open={dialogOpen}
        onClose={() => setDialogOpen(false)}
        title="¿Cancelar la póliza PE-2026-0876?"
        footer={
          <>
            <span />
            <div className="flex gap-2">
              <Button variant="secondary" onClick={() => setDialogOpen(false)}>
                Mantener
              </Button>
              <Button variant="danger" onClick={() => setDialogOpen(false)}>
                Cancelar póliza
              </Button>
            </div>
          </>
        }
      >
        <p className="text-sm text-muted">
          Se generará la póliza de reversa <b className="text-text">PD-2026-0520</b> por <b className="text-text">$64,000.00</b>. La
          original queda en el historial con estado «Cancelada».
        </p>
        <Checkbox defaultChecked>Notificar al responsable de cuentas por pagar</Checkbox>
      </Dialog>
    </div>
  )
}
