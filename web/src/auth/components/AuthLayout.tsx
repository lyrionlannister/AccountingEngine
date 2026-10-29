import type { ReactNode } from 'react'
import { Logo } from '../../ui/components/Logo'

export const AuthLayout = ({ title, description, children }: { title: string; description?: string; children: ReactNode }) => {
  return (
    <div className="grid min-h-screen place-items-center bg-bg p-4.5">
      <div className="grid w-full max-w-90 gap-6">
        <div className="flex items-center gap-2 text-text">
          <Logo className="size-6 text-primary" />
          <span className="font-head text-[15px] font-semibold">AccountingEngine</span>
        </div>

        <div className="grid gap-5 rounded-md border border-line bg-surface p-6 shadow-card">
          <div>
            <h1 className="font-head text-[20px] font-semibold text-text">{title}</h1>
            {description && <p className="mt-1 text-sm text-muted">{description}</p>}
          </div>
          {children}
        </div>
      </div>
    </div>
  )
}
