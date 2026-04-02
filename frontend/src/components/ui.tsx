import React from "react";

export type ButtonVariant = "primary" | "secondary" | "danger";

const buttonStyles: Record<ButtonVariant, string> = {
  primary:
    "bg-gradient-to-r from-[#ff7a18] via-[#ff4d6d] to-[#ffffff] text-white hover:from-[#ff8f2f] hover:via-[#ff5f7e] hover:to-[#ffffff] focus:ring-[#ff8a98] shadow-lg btn-glow",
  secondary:
    "bg-white text-slate-800 border border-pink-300 hover:bg-[#fff1f5] focus:ring-pink-200",
  danger:
    "bg-gradient-to-r from-rose-500 to-red-500 text-white hover:from-rose-600 hover:to-red-600 focus:ring-rose-300",
};

export const Button: React.FC<
  React.ButtonHTMLAttributes<HTMLButtonElement> & {
    variant?: ButtonVariant;
    icon?: React.ReactNode;
    fullWidth?: boolean;
  }
> = ({ variant = "primary", icon, fullWidth, className = "", children, ...rest }) => {
  return (
    <button
      className={`inline-flex items-center justify-center gap-2 rounded-lg px-4 py-2 text-sm font-semibold transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-offset-white ${buttonStyles[variant]} ${
        fullWidth ? "w-full" : ""
      } ${className}`}
      {...rest}
    >
      {icon}
      {children}
    </button>
  );
};

export const Input: React.FC<
  React.InputHTMLAttributes<HTMLInputElement> & { label?: string; error?: string; className?: string }
> = ({ label, error, className = "", ...rest }) => (
  <label className="space-y-2 text-sm text-slate-700">
    {label && <span className="font-medium text-slate-800">{label}</span>}
    <input
      className={`w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 placeholder:text-slate-400 shadow-sm transition focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100 ${className}`}
      {...rest}
    />
    {error && <p className="text-xs text-rose-600">{error}</p>}
  </label>
);

export const Select: React.FC<
  React.SelectHTMLAttributes<HTMLSelectElement> & { label?: string; error?: string; className?: string }
> = ({ label, error, className = "", children, ...rest }) => (
  <label className="space-y-2 text-sm text-slate-700">
    {label && <span className="font-medium text-slate-800">{label}</span>}
    <select
      className={`form-select w-full rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 shadow-sm transition focus:border-blue-500 focus:outline-none focus:ring-2 focus:ring-blue-100 ${className}`}
      {...rest}
    >
      {children}
    </select>
    {error && <p className="text-xs text-rose-600">{error}</p>}
  </label>
);

export const Card: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ className = "", ...rest }) => (
  <div className={`rounded-xl border border-slate-200 bg-white p-5 shadow-sm transition-all duration-300 hover:-translate-y-0.5 hover:shadow-lg ${className}`} {...rest} />
);

export const Badge: React.FC<{ status: "active" | "inactive" | "at-risk"; children: React.ReactNode }> = ({ status, children }) => {
  const classes = {
    active: "bg-emerald-100 text-emerald-700",
    inactive: "bg-slate-100 text-slate-500",
    "at-risk": "bg-amber-100 text-amber-700",
  };
  return <span className={`inline-flex items-center rounded-full px-2.5 py-1 text-xs font-semibold ${classes[status]}`}>{children}</span>;
};

export const TableWrapper: React.FC<React.HTMLAttributes<HTMLDivElement>> = ({ className = "", ...rest }) => (
  <div className={`overflow-x-auto rounded-xl border border-slate-200 bg-white shadow-sm ${className}`} {...rest} />
);

export const EmptyState: React.FC<{ title: string; description?: string }> = ({ title, description }) => (
  <div className="rounded-xl border border-dashed border-slate-300 bg-slate-50 p-8 text-center">
    <p className="text-lg font-semibold text-slate-700">{title}</p>
    {description && <p className="mt-2 text-sm text-slate-500">{description}</p>}
  </div>
);
