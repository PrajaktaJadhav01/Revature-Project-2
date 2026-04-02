import { useEffect, useState } from "react";
import api from "../api/client";

type AnalyticsSummary = {
  totalCustomers: number;
  atRiskCustomers: number;
  churnRiskPct: number;
  segmentationCounts: Record<string, number>;
  activeAccounts: number;
  revenue: number;
};

export function AnalyticsDashboardPage() {
  const [summary, setSummary] = useState<AnalyticsSummary | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const load = async () => {
      setLoading(true);
      setError(null);
      try {
        console.log("Fetching analytics from API...");
        let response;
        try {
          response = await api.get<AnalyticsSummary>("/analytics");
        } catch (apiErr) {
          console.warn("Try fallback route /customers/analytics/summary", apiErr);
          response = await api.get<AnalyticsSummary>("/customers/analytics/summary");
        }

        console.log("Analytics response:", response.data);
        setSummary(response.data);
      } catch (err: any) {
        console.error("Analytics fetch error:", err);
        const status = err?.response?.status;
        const apiMessage = err?.response?.data?.message || err?.response?.data || err?.message;

        if (status === 401) {
          setError("Unauthorized (401): Please sign in to access analytics.");
        } else if (status === 403) {
          setError("Forbidden (403): Insufficient permissions to view analytics.");
        } else {
          setError(`Failed to load analytics${status ? ` (${status})` : ""}. ${apiMessage || "Please refresh or contact support."}`.trim());
        }
      } finally {
        setLoading(false);
      }
    };

    void load();
  }, []);

  if (loading) {
    return (
      <div className="rounded-xl border border-slate-200 bg-white p-6 text-center shadow-sm">
        <div className="mx-auto mb-2 h-6 w-6 animate-spin rounded-full border-2 border-blue-500 border-t-transparent" />
        <p className="text-sm text-slate-600">Loading analytics...</p>
      </div>
    );
  }

  if (error) {
    return (
      <div className="rounded-xl border border-rose-200 bg-rose-50 p-5 text-sm text-rose-700">
        {error}
      </div>
    );
  }

  return (
    <div className="space-y-4">
      <div className="card card-boost">
        <h2 className="text-3xl font-bold text-slate-900">Analytics Dashboard</h2>
        <p className="text-sm text-slate-500">
          Live customer analytics (cached 5 minutes) for Sales and Management.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div className="card">
          <h3 className="text-lg font-semibold mb-3">Segmentation Distribution</h3>
          {summary ? (
            <div className="space-y-2">
              {Object.entries(summary.segmentationCounts).map(([segment, count]) => (
                <div key={segment} className="flex items-center justify-between text-sm">
                  <span>{segment}</span>
                  <span className="font-semibold">{count}</span>
                </div>
              ))}
            </div>
          ) : (
            <p>Loading...</p>
          )}
        </div>

        <div className="card">
          <h3 className="text-lg font-semibold mb-3">Churn Risk</h3>
          {summary ? (
            <div className="space-y-2 text-sm">
              <div>
                <span className="font-semibold">At Risk:</span> {summary.atRiskCustomers}
              </div>
              <div>
                <span className="font-semibold">Total Customers:</span> {summary.totalCustomers}
              </div>
              <div>
                <span className="font-semibold">Risk Rate:</span> {summary.churnRiskPct.toFixed(2)}%
              </div>
              <div>
                <span className="font-semibold">Active Accounts:</span> {summary.activeAccounts}
              </div>
              <div>
                <span className="font-semibold">Revenue:</span> ${summary.revenue.toLocaleString()}
              </div>
            </div>
          ) : (
            <p>Loading...</p>
          )}
        </div>
      </div>
    </div>
  );
}
