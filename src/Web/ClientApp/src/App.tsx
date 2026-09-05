import { Routes, Route } from 'react-router-dom';
import { AppLayout } from './layouts/AppLayout';
import { ProtectedRoute } from './features/auth/ProtectedRoute';
import { AppRoutes } from './app/routes';

export function App() {
  return (
    <Routes>
      {AppRoutes.map((route, index) => {
        const Layout = route.layout ?? AppLayout;
        let element = (
          <Layout>
            {route.element}
          </Layout>
        );

        if (route.protected) {
          element = <ProtectedRoute>{element}</ProtectedRoute>;
        }

        return (
          <Route
            key={index}
            path={route.path}
            element={element}
          />
        );
      })}
    </Routes>
  );
}
