import { useEffect, useState } from "react";
import { Link, useNavigate, useParams } from "react-router-dom";
import api from "../api/client";
import { classificationClass, healthClass } from "../utils/ui";
import { useAuth } from "../state/AuthContext";
import { Badge, Button, Card } from "../components/ui";

export function CustomerDetailPage() {
  const { customerId } = useParams();
  const navigate = useNavigate();
  const { role } = useAuth();

  const [customer, setCustomer] = useState<any>(null);
  const [health, setHealth] = useState<number | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!customerId) return;

    const load = async () => {
      try {
        const c = await api.get(`/customers/${customerId}`);
        setCustomer(c.data);

        if (role === "Admin" || role === "SalesManager") {
          const h = await api.get(`/customers/analytics/health-score`, { params: { customerId } });
          setHealth(h.data.score);
        }
      } catch (err: any) {
        console.error("Customer detail fetch error:", err);
        const status = err?.response?.status;
        if (status === 401) setError("Unauthorized (401). Please login.");
        else if (status === 403) setError("Forbidden (403). You are not allowed to view this customer.");
        else setError(`Failed to load customer details${status ? ` (${status})` : ""}. ${err?.response?.data || err?.message || "Please refresh."}`);
      }
    };

    void load();
  }, [customerId, role]);

  const onDelete = async () => {
    if (!customerId) return;
    if (!confirm("Delete customer?")) return;

    try {
      await api.delete(`/customers/${customerId}`);
      navigate("/", { replace: true });
    } catch (err: any) {
      alert(err?.response?.data ?? "Delete failed");
    }
  };

  if (error) return <p className="error">{error}</p>;
  if (!customer) return <p>Loading...</p>;

  return (
    <Card>
      <div className="mb-5 flex flex-wrap items-start justify-between gap-3 border-b border-slate-200 pb-4">
        <div>
          <h2 className="text-2xl font-semibold text-slate-900">{customer.customerName}</h2>
          <p className="text-sm text-slate-500">Customer profile and recent interactions.</p>
        </div>

        <div className="flex items-center gap-2">
          <Link to={`/customers/${customer.customerId}/edit`}>
            <Button variant="secondary">Edit</Button>
          </Link>
          {role === "Admin" && <Button variant="danger" onClick={onDelete}>Delete</Button>}
        </div>
      </div>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
        <div>
          <p className="text-xs font-medium text-slate-500">Email</p>
          <p className="text-sm font-semibold text-slate-900">{customer.email}</p>
        </div>

        <div>
          <p className="text-xs font-medium text-slate-500">Classification</p>
          <Badge status={customer.classification === "Active" ? "active" : customer.classification === "Inactive" ? "inactive" : "at-risk"}>
            {customer.classification}
          </Badge>
        </div>

        <div>
          <p className="text-xs font-medium text-slate-500">Segment</p>
          <p className="text-sm font-semibold text-slate-900">{customer.segment}</p>
        </div>

        {health !== null && (
          <div>
            <p className="text-xs font-medium text-slate-500">Health Score</p>
            <Badge status={health >= 75 ? "active" : health >= 50 ? "at-risk" : "inactive"}>{health}</Badge>
          </div>
        )}
      </div>

      <div className="mt-6 space-y-5">
        <section>
          <h3 className="text-lg font-semibold text-slate-900">Contacts</h3>
          {customer.contacts?.length ? (
            <ul className="mt-2 list-disc pl-5 text-sm text-slate-700 space-y-1">
              {customer.contacts.map((c: any) => (
                <li key={c.contactId}>{c.name} ({c.email}) {c.isPrimary ? <span className="font-semibold text-emerald-600">[Primary]</span> : null}</li>
              ))}
            </ul>
          ) : (
            <p className="mt-2 text-sm text-slate-500">No contacts available.</p>
          )}
        </section>

        <section>
          <h3 className="text-lg font-semibold text-slate-900">Addresses</h3>
          {customer.addresses?.length ? (
            <ul className="mt-2 list-disc pl-5 text-sm text-slate-700 space-y-1">
              {customer.addresses.map((a: any) => (
                <li key={a.addressId}>{a.street}, {a.city}, {a.state}, {a.postalCode}, {a.country} ({a.addressType})</li>
              ))}
            </ul>
          ) : (
            <p className="mt-2 text-sm text-slate-500">No addresses available.</p>
          )}
        </section>

        <section>
          <h3 className="text-lg font-semibold text-slate-900">Interactions</h3>
          {customer.interactions?.length ? (
            <ul className="mt-2 list-disc pl-5 text-sm text-slate-700 space-y-1">
              {customer.interactions.map((i: any) => (
                <li key={i.interactionId}>{i.interactionDate} - {i.type} - {i.subject}</li>
              ))}
            </ul>
          ) : (
            <p className="mt-2 text-sm text-slate-500">No interactions available.</p>
          )}
        </section>
      </div>
    </Card>
  );
}
