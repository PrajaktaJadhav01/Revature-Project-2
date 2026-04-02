import { Navigate, Route, Routes } from "react-router-dom";
import { Layout } from "./components/Layout";
import { ProtectedRoute } from "./components/ProtectedRoute";
import { AnalyticsDashboardPage } from "./pages/AnalyticsDashboardPage";
import { CustomerDetailPage } from "./pages/CustomerDetailPage";
import { CustomerFormPage } from "./pages/CustomerFormPage";
import { CustomerListPage } from "./pages/CustomerListPage";
import { LoginPage } from "./pages/LoginPage";

export default function App() {
  return (
    <Layout>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <CustomerListPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/customers/new"
          element={
            <ProtectedRoute>
              <CustomerFormPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/customers/:customerId/edit"
          element={
            <ProtectedRoute>
              <CustomerFormPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/customers/:customerId"
          element={
            <ProtectedRoute>
              <CustomerDetailPage />
            </ProtectedRoute>
          }
        />
        <Route
          path="/analytics"
          element={
            <ProtectedRoute roles={["Admin", "SalesManager"]}>
              <AnalyticsDashboardPage />
            </ProtectedRoute>
          }
        />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </Layout>
  );
}
