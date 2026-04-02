import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import api from "../api/client";
import { useAuth } from "../state/AuthContext";
import { Badge, Button, Card, Input, Select } from "../components/ui";

type CustomerPayload = {
  customerName: string;
  email: string;
  phone?: string;
  website?: string;
  industry?: string;
  companySize?: string;
  classification: string;
  type: string;
  segment: string;
  accountValue: number;
  assignedSalesRepId?: number | null;
};

export function CustomerFormPage() {
  const { customerId } = useParams();
  const isEdit = !!customerId && customerId !== "new";
  const navigate = useNavigate();
  const { role, assignedRepId } = useAuth();

  const [form, setForm] = useState<CustomerPayload>({
    customerName: "",
    email: "",
    phone: "",
    website: "",
    industry: "",
    companySize: "",
    classification: "Prospect",
    type: "Business",
    segment: "SMB",
    accountValue: 0,
    assignedSalesRepId: assignedRepId
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  useEffect(() => {
    if (!isEdit || !customerId) return;
    const load = async () => {
      setLoading(true);
      try {
        const { data } = await api.get(`/customers/${customerId}`);
        setForm({
          customerName: data.customerName,
          email: data.email,
          phone: data.phone ?? "",
          website: data.website ?? "",
          industry: data.industry ?? "",
          companySize: data.companySize ?? "",
          classification: data.classification,
          type: data.type,
          segment: data.segment,
          accountValue: data.accountValue,
          assignedSalesRepId: data.assignedSalesRepId
        });
      } catch {
        setError("Failed to load customer.");
      } finally {
        setLoading(false);
      }
    };
    void load();
  }, [isEdit, customerId]);

  const setField = <K extends keyof CustomerPayload>(key: K, value: CustomerPayload[K]) => {
    setForm((prev) => ({ ...prev, [key]: value }));
  };

  const onSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!form.customerName.trim()) {
      setError("Customer Name is required.");
      return;
    }
    if (!form.email.trim()) {
      setError("Email is required.");
      return;
    }
    if (form.accountValue < 0) {
      setError("Account Value must be 0 or greater.");
      return;
    }

    setError(null);
    setSuccess(null);

    console.time("onSubmitCustomer");
    setLoading(true);
    try {
      const payload = {
        ...form,
        assignedSalesRepId:
          role === "SalesRep" ? assignedRepId : form.assignedSalesRepId ?? null
      };

      if (isEdit && customerId) {
        await api.put(`/customers/${customerId}`, payload);
      } else {
        await api.post("/customers", payload);
      }

      console.log("Customer saved", payload);
      setSuccess("Customer saved successfully.");
      navigate("/", { replace: true });
    } catch (err: any) {
      console.error("Customer save error:", err);
      setError(err?.response?.data ?? "Save failed.");
    } finally {
      setLoading(false);
      console.timeEnd("onSubmitCustomer");
    }
  };

  const onCancel = () => {
    navigate("/", { replace: true });
  };

  return (
    <Card className="max-w-5xl mx-auto">
      <div className="mb-6 flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 pb-4">
        <div>
          <h2 className="text-2xl font-semibold text-slate-900">{isEdit ? "Edit Customer" : "Create Customer"}</h2>
          <p className="mt-1 text-sm text-slate-500">Use this form to add or update customer information quickly.</p>
        </div>
        <div className="rounded-full bg-gradient-to-r from-indigo-500 to-blue-500 px-3 py-1 text-xs font-semibold text-white">
          {isEdit ? "Update mode" : "New record"}
        </div>
      </div>

      <form onSubmit={onSubmit} className="grid grid-cols-1 gap-4 md:grid-cols-2">
        <Input label="Customer Name" required placeholder="Tech CRM Customer" value={form.customerName} onChange={(e) => setField("customerName", e.target.value)} />
        <Input label="Email" type="email" required placeholder="contact@techcrm.com" value={form.email} onChange={(e) => setField("email", e.target.value)} />
        <Input label="Phone" placeholder="(123) 456-7890" value={form.phone ?? ""} onChange={(e) => setField("phone", e.target.value)} />
        <Input label="Website" placeholder="https://" value={form.website ?? ""} onChange={(e) => setField("website", e.target.value)} />
        <Input label="Industry" placeholder="Technology" value={form.industry ?? ""} onChange={(e) => setField("industry", e.target.value)} />
        <Input label="Company Size" placeholder="e.g., 50-200" value={form.companySize ?? ""} onChange={(e) => setField("companySize", e.target.value)} />

        <div className="space-y-2">
          <Select label="Classification" value={form.classification} onChange={(e) => setField("classification", e.target.value)}>
            <option value="Prospect">Prospect</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="VIP">VIP</option>
            <option value="AtRisk">AtRisk</option>
          </Select>
        </div>
        <div className="space-y-2">
          <Select label="Type" value={form.type} onChange={(e) => setField("type", e.target.value)}>
            <option value="Business">Business</option>
            <option value="Individual">Individual</option>
          </Select>
        </div>
        <div className="space-y-2">
          <Select label="Segment" value={form.segment} onChange={(e) => setField("segment", e.target.value)}>
            <option value="Enterprise">Enterprise</option>
            <option value="MidMarket">MidMarket</option>
            <option value="SMB">SMB</option>
          </Select>
        </div>
        <div className="space-y-2">
          <Input label="Account Value" type="number" min={0} value={form.accountValue} onChange={(e) => setField("accountValue", Number(e.target.value))} />
        </div>

        {error && <p className="text-sm font-semibold text-rose-600 md:col-span-2">{error}</p>}
        {success && <p className="text-sm font-semibold text-emerald-600 md:col-span-2">{success}</p>}

        <div className="flex flex-wrap items-center gap-2 md:col-span-2 justify-end">
          <Button variant="secondary" type="button" onClick={onCancel} disabled={loading}>Cancel</Button>
          <Button variant="primary" type="submit" disabled={loading}>
            {loading ? (
              <>
                <span className="animate-spin inline-block w-4 h-4 mr-2 border-2 border-white border-t-transparent rounded-full" />
                Saving...
              </>
            ) : (
              "Save Customer"
            )}
          </Button>
        </div>
      </form>
    </Card>
  );
}
