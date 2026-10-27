import { render, screen } from '@testing-library/react'
import { MemoryRouter } from 'react-router'
import { describe, expect, it } from 'vitest'
import { App } from '../src/App'

describe('App', () => {
  it('redirects to the panel and renders the layout chrome', () => {
    render(
      <MemoryRouter initialEntries={['/']}>
        <App />
      </MemoryRouter>,
    )

    expect(screen.getByRole('heading', { name: 'Panel financiero' })).toBeInTheDocument()
    expect(screen.getAllByRole('link', { name: 'Pólizas' }).length).toBeGreaterThan(0)
  })
})
