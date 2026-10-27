import type { LogoProps } from '../props/logo-props'

export const Logo = ({ className = 'size-6' }: LogoProps) => {
  return (
    <svg viewBox="0 0 24 24" fill="currentColor" aria-hidden className={className}>
      <rect x="2" y="4" width="20" height="2.6" rx=".4" />
      <rect x="10.7" y="4" width="2.6" height="16" rx=".4" />
      <rect x="3.5" y="10" width="5.5" height="1.6" rx=".3" opacity=".55" />
      <rect x="3.5" y="13.4" width="4" height="1.6" rx=".3" opacity=".55" />
      <rect x="15" y="10" width="5.5" height="1.6" rx=".3" opacity=".55" />
    </svg>
  )
}
