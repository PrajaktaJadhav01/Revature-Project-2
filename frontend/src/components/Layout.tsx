import { Link, useLocation } from "react-router-dom";
import { useEffect, useState } from "react";
import { useAuth } from "../state/AuthContext";

const NavLink = ({
  to,
  children,
  icon
}: {
  to: string;
  children: React.ReactNode;
  icon?: React.ReactNode;
}) => {
  const location = useLocation();
  const active = location.pathname === to;
  return (
    <Link
      to={to}
      className={[
        "flex items-center gap-2 rounded-xl px-3 py-2 text-sm transition-all duration-200",
        active
          ? "bg-blue-600/20 text-blue-100 shadow-inner font-semibold"
          : "text-slate-300 hover:bg-blue-500/20 hover:text-white"
      ].join(" ")}
    >
      {icon && <span className="text-base">{icon}</span>}
      {children}
    </Link>
  );
};

export function Layout({ children }: { children: React.ReactNode }) {
  const { token, role, logout } = useAuth();
  const location = useLocation();
  const [activeSection, setActiveSection] = useState<string>(
    location.hash ? location.hash.replace('#', '') : 'home'
  );

  const navItems = [
    { name: 'Home', href: '#home' },
    { name: 'About', href: '#about' },
    { name: 'Services', href: '#services' },
    { name: 'CRM', href: '#crm' },
    { name: 'Contact', href: '#contact' }
  ];

  useEffect(() => {
    if (location.hash) {
      setActiveSection(location.hash.replace('#', ''));
    }
  }, [location.hash]);

  useEffect(() => {
    const root = document.querySelector('main');
    if (!root) return;

    const sections = navItems
      .map((item) => document.getElementById(item.href.replace('#', '')))
      .filter((el): el is HTMLElement => Boolean(el));

    if (sections.length === 0) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((entry) => entry.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio);

        if (visible.length > 0) {
          setActiveSection(visible[0].target.id);
          window.history.replaceState(null, '', `#${visible[0].target.id}`);
        }
      },
      {
        root,
        threshold: [0.3, 0.5, 0.7]
      }
    );

    sections.forEach((section) => observer.observe(section));
    return () => sections.forEach((section) => observer.unobserve(section));
  }, [navItems]);

  return (
    <div className="min-h-screen bg-slate-50">
      {token ? (
        <div className="flex min-h-screen flex-col">
          <header className="sticky top-0 z-50 bg-white/85 border-b border-slate-200 shadow-sm backdrop-blur-sm">
            <div className="mx-auto flex w-full max-w-7xl items-center justify-between px-4 py-3 md:px-8">
              <div className="flex items-center gap-4">
                <div className="font-extrabold text-xl text-gradient bg-clip-text text-transparent bg-gradient-to-r from-[#ff7a18] via-[#ff4d6d] to-[#ffffff]">Tech CRM</div>
                <nav className="hidden gap-4 text-sm font-medium text-slate-700 md:flex">
                  {navItems.map((item) => {
                    const sectionKey = item.href.replace('#', '');
                    const isActive = activeSection === sectionKey;
                    return (
                      <a
                        key={item.name}
                        href={item.href}
                        onClick={(e) => {
                          e.preventDefault();
                          const target = document.getElementById(sectionKey);
                          if (target) {
                            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
                            setActiveSection(sectionKey);
                            window.history.replaceState(null, '', item.href);
                          }
                        }}
                        className={`nav-link ${isActive ? 'active' : ''}`}
                      >
                        {item.name}
                      </a>
                    );
                  })}
                </nav>
              </div>
              <div className="flex items-center gap-3">
                <span className="hidden rounded-full bg-gradient-to-r from-[#ff7a18] via-[#ff4d6d] to-[#ffffff] px-3 py-1 text-xs font-semibold text-[#9f1239] md:inline-block">{role}</span>
                <button onClick={logout} className="rounded-lg bg-gradient-to-r from-[#ff7a18] via-[#ff4d6d] to-[#ffffff] px-3 py-1.5 text-sm font-semibold text-white hover:brightness-110 transition duration-300 shadow-sm">
                  Logout
                </button>
              </div>
            </div>
          </header>

          <div className="flex flex-1">
            <aside className="hidden w-72 shrink-0 bg-slate-900 text-slate-200 lg:block">
              <div className="border-b border-slate-800 px-6 py-5">
                <div className="text-lg font-bold text-white">CRM Dashboard</div>
                <div className="text-xs text-slate-400 mt-1">Business Overview</div>
              </div>
              <nav className="px-4 py-6 space-y-2">
                <NavLink to="/" icon="👥">Customers</NavLink>
                {(role === "Admin" || role === "SalesManager") && <NavLink to="/analytics" icon="📈">Analytics</NavLink>}
              </nav>
              <div className="px-4 pb-6">
                <div className="rounded-xl bg-slate-800/70 p-4">
                  <div className="text-xs text-slate-400">Role</div>
                  <div className="text-sm font-semibold text-white mt-1">{role}</div>
                </div>
              </div>
            </aside>

            <main className="flex-1 overflow-y-auto p-5 lg:p-8">
              {children}
            </main>
          </div>
        </div>
      ) : (
        <div className="min-h-screen flex items-center justify-center bg-slate-50 p-5">
          <main className="w-full max-w-6xl">{children}</main>
        </div>
      )}
    </div>
  );
}
