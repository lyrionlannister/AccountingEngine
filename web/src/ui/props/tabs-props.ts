import type { Tab } from '../types/tabs'

export type TabsProps = {
  tabs: Tab[]
  value: string
  onChange: (key: string) => void
  className?: string
}
