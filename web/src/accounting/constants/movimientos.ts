import type { Movimiento } from '../types/movimiento'

export const MOVIMIENTOS: Movimiento[] = [
  { id: '1', fecha: '2026-10-27', poliza: 'PE-2026-0878', concepto: 'Arrendamiento de oficinas', cuenta: '6103-04', estado: 'success', importe: -64000 },
  { id: '2', fecha: '2026-10-26', poliza: 'PI-2026-1142', concepto: 'Factura B-5521 · Inmobiliaria Reforma', cuenta: '4000-01', estado: 'success', importe: 182400 },
  { id: '3', fecha: '2026-10-24', poliza: 'PE-2026-0876', concepto: 'Nómina quincenal', cuenta: '6101-01', estado: 'warning', importe: -241300 },
  { id: '4', fecha: '2026-10-20', poliza: 'PD-2026-0520', concepto: 'Reversa póliza cancelada', cuenta: '1102-01', estado: 'error', importe: 64000 },
]
