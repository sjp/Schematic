import * as Dialog from "@radix-ui/react-dialog";
import { HeadContent, Outlet, useRouterState } from "@tanstack/react-router";
import {
  Boxes,
  Columns3,
  Database,
  Eye,
  KeyRound,
  LayoutDashboard,
  ListOrdered,
  ListTree,
  Menu,
  Replace,
  Search,
  Shapes,
  ShieldCheck,
  Share2,
  SquareFunction,
  Table2,
  Unlink,
  X,
  Zap,
} from "lucide-react";
import { useEffect, useState } from "react";

import { SearchCommand } from "@/components/SearchCommand";
import { useSummary } from "@/hooks/useReportData";
import { cn } from "@/lib/utils";
import type { LintSummary } from "@/types/report";

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
  { href: "#/schemas", path: "/schemas", label: "Schemas", icon: Boxes },
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
  {
    href: "#/user-defined-types",
    path: "/user-defined-types",
    label: "Types",
    icon: Shapes,
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

/** The product mark shown at the top of the sidebar and, on narrow screens, in the header. */
function Brand() {
  return (
    <>
      <Database className="size-5 text-sidebar-primary" />
      <span>Schematic</span>
    </>
  );
}

function NavLinks({
  pathname,
  lint,
  onNavigate,
}: {
  pathname: string;
  lint: LintSummary | undefined;
  /** Called when a link is followed, so the mobile drawer can close itself. */
  onNavigate?: () => void;
}) {
  return (
    <nav className="flex-1 space-y-0.5 overflow-y-auto p-2">
      {NAV.map((item) => {
        const isActive = item.path === "/" ? pathname === "/" : pathname.startsWith(item.path);
        const Icon = item.icon;
        return (
          <a
            key={item.href}
            href={item.href}
            onClick={onNavigate}
            aria-current={isActive ? "page" : undefined}
            className={cn(
              "flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors",
              isActive
                ? "bg-sidebar-accent text-sidebar-accent-foreground"
                : "text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground",
            )}
          >
            <Icon className="size-4" />
            {item.label}
            {item.path === "/lint" && lint !== undefined && lint.messageCount > 0 && (
              <span
                className={cn(
                  "ml-auto rounded-full px-1.5 py-0.5 text-xs font-medium tabular-nums",
                  lint.errorCount > 0
                    ? "bg-destructive/15 text-destructive"
                    : "bg-sidebar-accent text-sidebar-foreground/80",
                )}
              >
                {lint.messageCount}
              </span>
            )}
          </a>
        );
      })}
    </nav>
  );
}

export function RootLayout() {
  const pathname = useRouterState({ select: (s) => s.location.pathname });
  const [searchOpen, setSearchOpen] = useState(false);
  // Below the `md` breakpoint the sidebar is hidden and the same links live in a drawer.
  const [navOpen, setNavOpen] = useState(false);
  // One cached query shared with the lint page and every detail page, so the badge costs
  // nothing beyond the first load.
  const { data: lint } = useSummary<LintSummary>("lint");

  useEffect(() => {
    function onKey(e: KeyboardEvent) {
      if ((e.metaKey || e.ctrlKey) && e.key.toLowerCase() === "k") {
        e.preventDefault();
        setSearchOpen((o) => !o);
      }
    }
    document.addEventListener("keydown", onKey);
    return () => {
      document.removeEventListener("keydown", onKey);
    };
  }, []);

  // The drawer is hidden from `md` up, but a modal dialog would still lock page scrolling, so
  // close it when the viewport widens past the breakpoint (e.g. a tablet rotating).
  useEffect(() => {
    const wide = window.matchMedia("(min-width: 48rem)");
    function onChange(e: MediaQueryListEvent) {
      if (e.matches) {
        setNavOpen(false);
      }
    }
    wide.addEventListener("change", onChange);
    return () => {
      wide.removeEventListener("change", onChange);
    };
  }, []);

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      {/* Renders the active route's <title> (hoisted to <head> by React 19). */}
      <HeadContent />
      {/* Sticky so the links stay in reach however far down a long page is scrolled. */}
      <aside className="sticky top-0 hidden h-screen w-60 shrink-0 flex-col border-r bg-sidebar text-sidebar-foreground md:flex">
        <div className="flex h-14 shrink-0 items-center gap-2 border-b px-4 font-semibold">
          <Brand />
        </div>
        <NavLinks pathname={pathname} lint={lint} />
      </aside>

      <Dialog.Root open={navOpen} onOpenChange={setNavOpen}>
        <Dialog.Portal>
          <Dialog.Overlay className="fixed inset-0 z-40 bg-black/50 data-[state=closed]:animate-out data-[state=closed]:fade-out-0 data-[state=open]:animate-in data-[state=open]:fade-in-0 md:hidden" />
          <Dialog.Content
            aria-describedby={undefined}
            className="fixed inset-y-0 left-0 z-50 flex w-72 max-w-[85vw] flex-col border-r bg-sidebar text-sidebar-foreground shadow-lg data-[state=closed]:animate-out data-[state=closed]:slide-out-to-left data-[state=open]:animate-in data-[state=open]:slide-in-from-left md:hidden"
          >
            <div className="flex h-14 shrink-0 items-center gap-2 border-b px-4 font-semibold">
              <Brand />
              <Dialog.Close
                className="-mr-2 ml-auto rounded-md p-2 text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground"
                aria-label="Close navigation"
              >
                <X className="size-5" />
              </Dialog.Close>
            </div>
            <Dialog.Title className="sr-only">Navigation</Dialog.Title>
            <NavLinks
              pathname={pathname}
              lint={lint}
              onNavigate={() => {
                setNavOpen(false);
              }}
            />
          </Dialog.Content>
        </Dialog.Portal>
      </Dialog.Root>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="sticky top-0 z-30 flex h-14 shrink-0 items-center gap-2 border-b bg-background px-4 md:px-6">
          <button
            type="button"
            onClick={() => {
              setNavOpen(true);
            }}
            className="-ml-2 rounded-md p-2 text-muted-foreground transition-colors hover:bg-accent hover:text-foreground md:hidden"
            aria-label="Open navigation"
          >
            <Menu className="size-5" />
          </button>
          <div className="flex items-center gap-2 font-semibold md:hidden">
            <Brand />
          </div>
          <div className="hidden text-sm text-muted-foreground md:block">
            Database schema report
          </div>
          <button
            type="button"
            onClick={() => {
              setSearchOpen(true);
            }}
            className="ml-auto flex items-center gap-2 rounded-md border px-3 py-1.5 text-sm text-muted-foreground transition-colors hover:text-foreground"
            aria-label="Search schema"
          >
            <Search className="size-4" />
            <span>Search</span>
            {/* A keyboard shortcut means nothing on a phone. */}
            <kbd className="ml-2 hidden rounded bg-muted px-1.5 py-0.5 text-xs font-medium text-muted-foreground md:inline">
              ⌘K
            </kbd>
          </button>
        </header>
        <main className="min-w-0 flex-1 p-4 md:p-6">
          <Outlet />
        </main>
      </div>

      <SearchCommand open={searchOpen} onOpenChange={setSearchOpen} />
    </div>
  );
}
