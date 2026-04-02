import { useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import api from "../api/client";
import { classificationLabel, segmentLabel } from "../utils/ui";
import { useAuth } from "../state/AuthContext";
import { Button, Card, EmptyState, Input, Select, TableWrapper } from "../components/ui";
import { IconDelete, IconEdit, IconPlus } from "../components/icons";

type Customer = {
  customerId: number;
  customerName: string;
  email: string;
  phone?: string | null;
  classification: unknown;
  segment: unknown;
  accountValue: number;
};

type CustomersPage = {
  items: Customer[];
  totalCount: number;
};

type ToastState = { type: "success" | "error"; message: string } | null;

export function CustomerListPage() {
  const { role } = useAuth();

  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);

  const [query, setQuery] = useState("");
  const [classificationFilter, setClassificationFilter] = useState("All");
  const [segmentFilter, setSegmentFilter] = useState("All");

  const [data, setData] = useState<CustomersPage>({ items: [], totalCount: 0 });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [toast, setToast] = useState<ToastState>(null);
  const [deletingId, setDeletingId] = useState<number | null>(null);

  const loadCustomers = async () => {
    setLoading(true);
    setError(null);
    try {
      const res = await api.get<CustomersPage>("/customers", {
        params: { pageNumber: page, pageSize }
      });

      if (!res.data || !Array.isArray(res.data.items)) {
        throw new Error("Invalid response payload from customers endpoint.");
      }

      setData(res.data);
    } catch (e: any) {
      console.error("Customers fetch error:", e);
      const status = e?.response?.status;
      const apiMessage = e?.response?.data?.message || e?.response?.data || e?.message;
      let statusMessage = "";

      if (status === 401) statusMessage = "Authentication required. Please log in.";
      else if (status === 403) statusMessage = "You don't have permission to view customers.";
      else if (status >= 500) statusMessage = "Server error. Please try again later.";

      setError(`Failed to load customers${status ? ` (${status})` : ""}. ${statusMessage} ${apiMessage || "Please refresh or contact support."}`.trim());
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    void loadCustomers();
  }, [page, pageSize]);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    return data.items.filter((c) => {
      const matchQuery =
        !q ||
        c.customerName.toLowerCase().includes(q) ||
        c.email.toLowerCase().includes(q);

      const matchClassification =
        classificationFilter === "All" ||
        classificationLabel(c.classification) === classificationFilter;

      const matchSegment =
        segmentFilter === "All" || segmentLabel(c.segment) === segmentFilter;

      return matchQuery && matchClassification && matchSegment;
    });
  }, [data.items, query, classificationFilter, segmentFilter]);

  const totalPages = useMemo(() => {
    return Math.max(1, Math.ceil(data.totalCount / pageSize));
  }, [data.totalCount, pageSize]);

  useEffect(() => {
    // Reset pagination for a more predictable UX when filters change.
    setPage(1);
  }, [query, classificationFilter, segmentFilter]);

  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add("show");
          }
        });
      },
      { threshold: 0.25 }
    );

    document.querySelectorAll(".fade-in").forEach((el) => observer.observe(el));
    return () => {
      document.querySelectorAll(".fade-in").forEach((el) => observer.unobserve(el));
    };
  }, []);

  const showToast = (type: ToastState["type"], message: string) => {
    setToast({ type, message });
    window.setTimeout(() => setToast(null), 3000);
  };

  const handleDelete = async (customerId: number) => {
    if (!confirm("Delete customer?")) return;
    try {
      setDeletingId(customerId);
      await api.delete(`/customers/${customerId}`);
      showToast("success", "Customer deleted.");
      setData((prev) => ({
        ...prev,
        items: prev.items.filter((x) => x.customerId !== customerId)
      }));
    } catch (e: any) {
      const msg = e?.response?.data || "Delete failed.";
      showToast("error", msg);
    } finally {
      setDeletingId(null);
    }
  };

  return (
    <div className="space-y-8">
      <section id="home" className="relative overflow-hidden rounded-2xl hero-gradient p-8 text-slate-900 shadow-xl">
        <div className="absolute inset-0 bg-black/20" />
        <div className="relative z-10 max-w-5xl space-y-4">
          <p className="text-sm font-semibold uppercase tracking-widest text-[#d74b62]">Enterprise CRM</p>
          <h1 className="text-5xl font-extrabold leading-tight md:text-7xl text-white">GROWTH</h1>
          <p className="max-w-2xl text-lg text-white/90 md:text-xl">
            Scale customer relationships with modern analytics, seamless workflows, and premium UX.
          </p>
          <div className="flex flex-wrap gap-3">
            <Link to="/customers/new" className="btn-glow inline-flex items-center rounded-lg bg-gradient-to-r from-[#ff7a18] via-[#ff4d6d] to-[#ffffff] px-6 py-3 text-sm font-bold text-white shadow-lg transition hover:scale-[1.02] hover:-translate-y-0.5">
              Get Started
            </Link>
            <a href="#about" className="inline-flex items-center rounded-lg border border-white/60 bg-white/15 px-6 py-3 text-sm font-semibold text-white transition hover:bg-white/25">
              Learn More
            </a>
          </div>
        </div>
      </section>

      <section id="about" className="fade-in rounded-2xl border border-slate-200 bg-white p-8 shadow-lg">
        <h2 className="text-3xl font-bold text-slate-900">Transform customer relationships</h2>
        <p className="mt-2 text-sm text-slate-600 md:text-base">Tech CRM empowers enterprise teams with connected sales, support and analytics workflows, in a single platform optimized for growth.</p>
      </section>

      <section id="services" className="fade-in rounded-2xl border border-slate-200 bg-white p-8 shadow-lg">
        <h2 className="text-3xl font-bold text-slate-900">Services & Features</h2>
        <div className="mt-5 grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          <div className="card-boost p-6">
            <div className="text-3xl">🔍</div>
            <h3 className="mt-3 text-lg font-semibold text-slate-900">Customer 360</h3>
            <p className="mt-1 text-sm text-slate-600">Complete customer view with timelines, interactions, and revenue signals.</p>
          </div>
          <div className="card-boost p-6">
            <div className="text-3xl">⚙️</div>
            <h3 className="mt-3 text-lg font-semibold text-slate-900">Workflow Automation</h3>
            <p className="mt-1 text-sm text-slate-600">Automated task orchestration and SLA tracking for support and sales teams.</p>
          </div>
          <div className="card-boost p-6">
            <div className="text-3xl">📊</div>
            <h3 className="mt-3 text-lg font-semibold text-slate-900">Smart Analytics</h3>
            <p className="mt-1 text-sm text-slate-600">Live dashboards with health scores, churn predictions, and executive trends.</p>
          </div>
        </div>
      </section>

      <div className="fade-in grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-2">
        <button onClick={() => window.scrollTo({ top: document.getElementById('crm')?.offsetTop ?? 0, behavior: 'smooth' })} className="card-boost text-left p-6 transition cursor-pointer">
          <p className="text-xs font-semibold uppercase text-cyan-500">Quick access</p>
          <h3 className="mt-3 text-xl font-bold text-slate-900">Go to Customer List</h3>
          <p className="mt-1 text-sm text-slate-600">Manage accounts, pipelines and priorities instantly.</p>
        </button>
        <Link to="/analytics" className="card-boost block p-6 transition">
          <p className="text-xs font-semibold uppercase text-cyan-500">Insights</p>
          <h3 className="mt-3 text-xl font-bold text-slate-900">Open Analytics</h3>
          <p className="mt-1 text-sm text-slate-600">Review KPI metrics and performance snapshots.</p>
        </Link>
      </div>

      <section className="fade-in rounded-2xl border border-slate-200 bg-white p-8 shadow-lg">
        <h2 className="text-3xl font-bold text-slate-900">Stay in the loop</h2>
        <p className="mt-2 text-sm text-slate-600">Subscribe to our newsletter for product updates, best practices, and growth strategies.</p>
        <form className="mt-4 flex flex-col gap-2 sm:flex-row">
          <Input label="Email" type="email" placeholder="your.email@company.com" className="flex-1" />
          <button type="submit" className="rounded-lg bg-[#0A66FF] px-5 py-2.5 text-sm font-bold text-white transition hover:bg-[#084bdb] btn-glow">Subscribe</button>
        </form>
      </section>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:-translate-y-0.5 hover:shadow-lg">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">Total Customers</div>
          <div className="mt-3 text-2xl font-bold text-slate-900">{data.totalCount}</div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:-translate-y-0.5 hover:shadow-lg">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">Active Accounts</div>
          <div className="mt-3 text-2xl font-bold text-slate-900">{data.items.filter(x => classificationLabel(x.classification) === 'Active').length}</div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:-translate-y-0.5 hover:shadow-lg">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">SMB Segment</div>
          <div className="mt-3 text-2xl font-bold text-slate-900">{data.items.filter(x => segmentLabel(x.segment) === 'SMB').length}</div>
        </div>
        <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm transition hover:-translate-y-0.5 hover:shadow-lg">
          <div className="text-xs font-semibold uppercase tracking-wide text-slate-500">Enterprise Segment</div>
          <div className="mt-3 text-2xl font-bold text-slate-900">{data.items.filter(x => segmentLabel(x.segment) === 'Enterprise').length}</div>
        </div>
      </div>

      <div id="crm" className="flex flex-col gap-4 md:flex-row md:items-center md:justify-between">
        <div>
          <h2 className="text-3xl font-semibold tracking-tight text-slate-900">Customers</h2>
          <p className="mt-1 text-sm text-slate-500">Manage and segment customer accounts, track health status, and take action.</p>
        </div>

        <Link to="/customers/new" className="inline-flex">
          <Button variant="primary" icon={<IconPlus />}>Create Customer</Button>
        </Link>
      </div>

      {toast && (
        <div
          className={[
            "rounded-xl px-4 py-3 text-sm font-medium border",
            toast.type === "success"
              ? "bg-emerald-50 border-emerald-200 text-emerald-800"
              : "bg-red-50 border-red-200 text-red-700"
          ].join(" ")}
          role="status"
        >
          {toast.message}
        </div>
      )}

      <Card>
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <Input label="Search" placeholder="Name or email" value={query} onChange={(e) => setQuery(e.target.value)} />
          <Select label="Classification" value={classificationFilter} onChange={(e) => setClassificationFilter(e.target.value)}>
            <option value="All">All</option>
            <option value="Prospect">Prospect</option>
            <option value="Active">Active</option>
            <option value="Inactive">Inactive</option>
            <option value="VIP">VIP</option>
            <option value="AtRisk">AtRisk</option>
          </Select>
          <Select label="Segment" value={segmentFilter} onChange={(e) => setSegmentFilter(e.target.value)}>
            <option value="All">All</option>
            <option value="Enterprise">Enterprise</option>
            <option value="MidMarket">MidMarket</option>
            <option value="SMB">SMB</option>
          </Select>
          <Select label="Page size" value={`${pageSize}`} onChange={(e) => setPageSize(Number(e.target.value))}>
            <option value={10}>10</option>
            <option value={20}>20</option>
            <option value={50}>50</option>
          </Select>
        </div>

        {error && <div className="mt-3 text-sm text-rose-600">{error}</div>}
      </Card>

      <Card>
        <div className="mb-4 flex flex-wrap items-center justify-between gap-2">
          <span className="text-sm text-slate-600">
            Showing <strong className="text-slate-900">{filtered.length}</strong> of <strong className="text-slate-900">{data.totalCount}</strong> customers
          </span>
          <span className="text-sm text-slate-500">Role: <strong className="text-slate-700">{role}</strong></span>
        </div>

        {loading ? (
          <div className="rounded-lg border border-dashed border-slate-300 bg-slate-50 p-8 text-center text-slate-500">Loading customers...</div>
        ) : filtered.length === 0 ? (
          <EmptyState title="No customers found" description="Adjust search or filters to find customers." />
        ) : (
          <TableWrapper>
            <table className="min-w-[900px] w-full border-separate border-spacing-y-2 text-sm">
              <thead className="bg-slate-50">
                <tr className="text-left text-xs font-semibold uppercase tracking-wider text-slate-600">
                  <th className="px-4 py-3">Customer</th>
                  <th className="px-4 py-3">Classification</th>
                  <th className="px-4 py-3">Segment</th>
                  <th className="px-4 py-3">Account Value</th>
                  <th className="px-4 py-3">Actions</th>
                </tr>
              </thead>
              <tbody>
                {filtered.map((c) => (
                  <tr key={c.customerId} className="bg-white transition table-row-hover hover:shadow-lg">
                    <td className="px-4 py-4">
                      <p className="font-semibold text-slate-900">{c.customerName}</p>
                      <p className="text-xs text-slate-500">{c.email}</p>
                    </td>
                    <td className="px-4 py-4">
                      <span className="rounded-full bg-emerald-100 px-2 py-1 text-xs font-semibold text-emerald-700">{classificationLabel(c.classification)}</span>
                    </td>
                    <td className="px-4 py-4 text-slate-700">{segmentLabel(c.segment)}</td>
                    <td className="px-4 py-4 font-semibold text-slate-900">${Number(c.accountValue).toLocaleString()}</td>
                    <td className="px-4 py-4">
                      <div className="flex items-center gap-2">
                        <Link to={`/customers/${c.customerId}`}>
                          <Button variant="secondary" className="text-xs px-2.5 py-1.5">View</Button>
                        </Link>
                        <Link to={`/customers/${c.customerId}/edit`}>
                          <Button variant="secondary" className="text-xs px-2.5 py-1.5">Edit</Button>
                        </Link>
                        <Button
                          variant="danger"
                          onClick={() => handleDelete(c.customerId)}
                          disabled={deletingId === c.customerId}
                          className="text-xs px-2.5 py-1.5"
                        >
                          {deletingId === c.customerId ? "Deleting..." : "Delete"}
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </TableWrapper>
        )}

        <div className="mt-4 flex flex-wrap items-center justify-between gap-2 text-sm text-slate-600">
          <span>
            Page <strong className="text-slate-800">{page}</strong> of <strong className="text-slate-800">{totalPages}</strong>
          </span>
          <div className="flex gap-2">
            <Button variant="secondary" onClick={() => setPage((p) => Math.max(1, p - 1))} disabled={page === 1 || loading}>
              Prev
            </Button>
            <Button variant="secondary" onClick={() => setPage((p) => Math.min(totalPages, p + 1))} disabled={page === totalPages || loading}>
              Next
            </Button>
          </div>
        </div>
      </Card>

      <section id="contact" className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
        <h2 className="text-2xl font-semibold text-slate-900">Contact Sales</h2>
        <p className="mt-2 text-sm text-slate-600">Need help deploying Tech CRM at scale? Reach out to our team and we’ll connect you with a dedicated solution consultant.</p>
        <div className="mt-4 grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
          <div className="rounded-lg border border-slate-200 p-4">
            <div className="text-xs font-semibold uppercase text-slate-400">Email</div>
            <div className="mt-1 text-sm font-semibold text-slate-700">sales@techcrm.com</div>
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <div className="text-xs font-semibold uppercase text-slate-400">Phone</div>
            <div className="mt-1 text-sm font-semibold text-slate-700">+1 (800) 555-0199</div>
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <div className="text-xs font-semibold uppercase text-slate-400">Location</div>
            <div className="mt-1 text-sm font-semibold text-slate-700">123 Business Ave, Tech City</div>
          </div>
          <div className="rounded-lg border border-slate-200 p-4">
            <div className="text-xs font-semibold uppercase text-slate-400">Office Hours</div>
            <div className="mt-1 text-sm font-semibold text-slate-700">Mon-Fri, 9am-6pm</div>
          </div>
        </div>
      </section>
    </div>
  );
}
