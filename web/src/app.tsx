import { Navigate, Route, Routes } from 'react-router'
import { LoginPage } from './auth/pages/login-page'
import { ProtectedRoute } from './auth/components/protected-route'
import { AppLayout } from './layout/components/app-layout'
import { PanelPage } from './accounting/pages/panel-page'
import { PolizasPage } from './accounting/pages/polizas-page'
import { RegisterPage } from './users/pages/register-page'

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
        </Route>
      </Route>
    </Routes>
  )
}
