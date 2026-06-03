import { useEffect, useState } from "react";
import { HeadContent, Outlet, useRouterState } from "@tanstack/react-router";
import {
  Columns3,
  Database,
  Eye,
  KeyRound,
  LayoutDashboard,
  ListOrdered,
  ListTree,
  Replace,
  Search,
  ShieldCheck,
  Share2,
  SquareFunction,
  Table2,
  Unlink,
  Zap,
} from "lucide-react";
import { SearchCommand } from "@/components/SearchCommand";
import { cn } from "@/lib/utils";

type NavItem = {
  /** Hash-route fragment (e.g. `#/tables`). */
  href: string;
  /** Path the route resolves to, used for active-state matching. */
  path: string;
  label: string;
  icon: typeof Database;
};

// Plain hash anchors keep the sidebar decoupled from the typed route tree:
// later waves register the real routes; the links already point at them.
const NAV: NavItem[] = [
  { href: "#/", path: "/", label: "Dashboard", icon: LayoutDashboard },
  { href: "#/tables", path: "/tables", label: "Tables", icon: Table2 },
  { href: "#/views", path: "/views", label: "Views", icon: Eye },
  {
    href: "#/sequences",
    path: "/sequences",
    label: "Sequences",
    icon: ListOrdered,
  },
  { href: "#/synonyms", path: "/synonyms", label: "Synonyms", icon: Replace },
  {
    href: "#/routines",
    path: "/routines",
    label: "Routines",
    icon: SquareFunction,
  },
  { href: "#/triggers", path: "/triggers", label: "Triggers", icon: Zap },
  { href: "#/columns", path: "/columns", label: "Columns", icon: Columns3 },
  {
    href: "#/constraints",
    path: "/constraints",
    label: "Constraints",
    icon: KeyRound,
  },
  { href: "#/indexes", path: "/indexes", label: "Indexes", icon: ListTree },
  {
    href: "#/relationships",
    path: "/relationships",
    label: "Relationships",
    icon: Share2,
  },
  { href: "#/orphans", path: "/orphans", label: "Orphans", icon: Unlink },
  { href: "#/lint", path: "/lint", label: "Lint", icon: ShieldCheck },
];

export function RootLayout() {
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const [searchOpen, setSearchOpen] = useState(false);

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        setSearchOpen((o) => !o);
      }
    }
    document.addEventListener("keydown", onKey);
    return () => document.removeEventListener("keydown", onKey);
  }, []);

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      {/* Renders the active route's <title> (hoisted to <head> by React 19). */}
      <HeadContent />
      <aside className="flex w-60 shrink-0 flex-col border-r bg-sidebar text-sidebar-foreground">
        <div className="flex h-14 items-center gap-2 border-b px-4 font-semibold">
          <Database className="size-5 text-sidebar-primary" />
          <span>Schematic</span>
        </div>
        <nav className="flex-1 space-y-0.5 overflow-y-auto p-2">
          {NAV.map((item) => {
            const isActive =
              item.path === "/"
                ? pathname === "/"
                : pathname.startsWith(item.path);
            const Icon = item.icon;
            return (
              <a
                key={item.href}
                href={item.href}
                className={cn(
                  "flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors",
                  isActive
                    ? "bg-sidebar-accent text-sidebar-accent-foreground"
                    : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
                )}
              >
                <Icon className="size-4" />
                {item.label}
              </a>
            );
          })}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex h-14 shrink-0 items-center justify-between border-b px-6">
          <div className="text-sm text-muted-foreground">
            Database schema report
          </div>
          <button
            type="button"
            onClick={() => setSearchOpen(true)}
            className="text-muted-foreground hover:text-foreground flex items-center gap-2 rounded-md border px-3 py-1.5 text-sm transition-colors"
            aria-label="Search schema"
          >
            <Search className="size-4" />
            <span>Search</span>
            <kbd className="bg-muted text-muted-foreground ml-2 rounded px-1.5 py-0.5 text-xs font-medium">
              ⌘K
            </kbd>
          </button>
        </header>
        <main className="min-w-0 flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>

      <SearchCommand open={searchOpen} onOpenChange={setSearchOpen} />
    </div>
  );
}
