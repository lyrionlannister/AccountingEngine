export type Movimiento = {
  id: string
  fecha: string
  poliza: string
  concepto: string
  cuenta: string
  estado: 'success' | 'warning' | 'error'
  importe: number
}
