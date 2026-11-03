import { Navigate, Route, Routes } from 'react-router'
import { LoginPage } from './auth/pages/LoginPage'
import { ProtectedRoute } from './auth/components/ProtectedRoute'
import { AppLayout } from './layout/components/AppLayout'
import { PanelPage } from './accounting/pages/PanelPage'
import { PolizasPage } from './accounting/pages/PolizasPage'
import { ComponentesPage } from './layout/pages/ComponentesPage'
import { RegisterPage } from './users/pages/RegisterPage'

export const App = () => {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route path="/registro" element={<RegisterPage />} />

      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route index element={<Navigate to="/panel" replace />} />
          <Route path="/panel" element={<PanelPage />} />
          <Route path="/polizas" element={<PolizasPage />} />
          <Route path="/componentes" element={<ComponentesPage />} />
        </Route>
      </Route>
    </Routes>
  )
}
