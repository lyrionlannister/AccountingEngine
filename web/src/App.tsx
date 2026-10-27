import { Navigate, Route, Routes } from 'react-router'
import { AppLayout } from './layout/components/AppLayout'
import { PanelPage } from './accounting/pages/PanelPage'
import { PolizasPage } from './accounting/pages/PolizasPage'
import { ComponentesPage } from './layout/pages/ComponentesPage'

export const App = () => {
  return (
    <Routes>
      <Route element={<AppLayout />}>
        <Route index element={<Navigate to="/panel" replace />} />
        <Route path="/panel" element={<PanelPage />} />
        <Route path="/polizas" element={<PolizasPage />} />
        <Route path="/componentes" element={<ComponentesPage />} />
      </Route>
    </Routes>
  )
}
