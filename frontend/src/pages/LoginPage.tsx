import { useState } from "react";
import { useNavigate } from "react-router-dom";
import api from "../api/client";
import { useAuth } from "../state/AuthContext";
import { Button, Card, Input } from "../components/ui";

type LoginResponse = {
  token: string;
  userId: number;
  role: "SalesRep" | "SalesManager" | "Admin";
  assignedRepId?: number | null;
};

export function LoginPage() {
  const [username, setUsername] = useState("admin");
  const [password, setPassword] = useState("Admin@123");
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();
  const { login } = useAuth();

  const rolePresets = [
    { key: "Admin", label: "Admin", username: "admin", password: "Admin@123" },
    { key: "SalesManager", label: "Sales Manager", username: "manager", password: "Manager@123" },
    { key: "SalesRep", label: "Sales Rep", username: "rep", password: "Rep@123" }
  ];

  const applyPreset = (preset: { username: string; password: string }) => {
    setUsername(preset.username);
    setPassword(preset.password);
    setError(null);
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      const { data } = await api.post<LoginResponse>("/auth/login", { username, password });
      login({
        token: data.token,
        role: data.role,
        userId: data.userId,
        assignedRepId: data.assignedRepId ?? null
      });
      navigate("/", { replace: true });
    } catch (err: any) {
      if (err?.response?.status === 401) {
        setError("Invalid username or password.");
      } else if (err?.request) {
        setError("Cannot connect to API. Confirm server is running and CORS is allowed.");
      } else {
        setError("Login failed. " + (err?.message ?? "Unknown error."));
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="narrow">
      <Card className="max-w-lg mx-auto">
        <div className="mb-4">
          <h2 className="text-2xl font-bold text-slate-900">Welcome Back</h2>
          <p className="text-sm text-slate-500">Sign in to continue to your Customer Management Dashboard</p>
        </div>

        <div className="mb-3">
          <p className="text-sm text-slate-600 mb-2">Quick login (select one to autofill):</p>
          <div className="flex flex-wrap gap-2">
            {rolePresets.map((preset) => (
              <Button
                key={preset.key}
                type="button"
                variant="secondary"
                onClick={() => applyPreset(preset)}
              >
                {preset.label}
              </Button>
            ))}
          </div>
        </div>

        <form onSubmit={onSubmit} className="space-y-4">
          <Input label="Username" value={username} onChange={(e) => setUsername(e.target.value)} />
          <Input label="Password" type="password" value={password} onChange={(e) => setPassword(e.target.value)} />

          {error && <div className="rounded-lg bg-rose-50 border border-rose-200 px-3 py-2 text-sm text-rose-700">{error}</div>}

          <div className="flex justify-between items-center">
            <span className="text-xs text-slate-500">Tips:
              admin/Admin@123, manager/Manager@123, rep/Rep@123
            </span>
            <Button type="submit" variant="primary" disabled={loading}>
              {loading ? "Signing in..." : "Sign in"}
            </Button>
          </div>
        </form>
      </Card>
    </div>
  );
}
