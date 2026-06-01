import { Outlet, useRouterState } from '@tanstack/react-router'
import {
  Columns3,
  Database,
  Eye,
  KeyRound,
  LayoutDashboard,
  ListOrdered,
  ListTree,
  Replace,
  ShieldCheck,
  Share2,
  SquareFunction,
  Table2,
  Unlink,
  Zap,
} from 'lucide-react'
import { cn } from '@/lib/utils'

type NavItem = {
  /** Hash-route fragment (e.g. `#/tables`). */
  href: string
  /** Path the route resolves to, used for active-state matching. */
  path: string
  label: string
  icon: typeof Database
}

// Plain hash anchors keep the sidebar decoupled from the typed route tree:
// later waves register the real routes; the links already point at them.
const NAV: NavItem[] = [
  { href: '#/', path: '/', label: 'Dashboard', icon: LayoutDashboard },
  { href: '#/tables', path: '/tables', label: 'Tables', icon: Table2 },
  { href: '#/views', path: '/views', label: 'Views', icon: Eye },
  { href: '#/sequences', path: '/sequences', label: 'Sequences', icon: ListOrdered },
  { href: '#/synonyms', path: '/synonyms', label: 'Synonyms', icon: Replace },
  { href: '#/routines', path: '/routines', label: 'Routines', icon: SquareFunction },
  { href: '#/triggers', path: '/triggers', label: 'Triggers', icon: Zap },
  { href: '#/columns', path: '/columns', label: 'Columns', icon: Columns3 },
  { href: '#/constraints', path: '/constraints', label: 'Constraints', icon: KeyRound },
  { href: '#/indexes', path: '/indexes', label: 'Indexes', icon: ListTree },
  { href: '#/relationships', path: '/relationships', label: 'Relationships', icon: Share2 },
  { href: '#/orphans', path: '/orphans', label: 'Orphans', icon: Unlink },
  { href: '#/lint', path: '/lint', label: 'Lint', icon: ShieldCheck },
]

export function RootLayout() {
  const pathname = useRouterState({ select: (s) => s.location.pathname })

  return (
    <div className="flex min-h-screen bg-background text-foreground">
      <aside className="flex w-60 shrink-0 flex-col border-r bg-sidebar text-sidebar-foreground">
        <div className="flex h-14 items-center gap-2 border-b px-4 font-semibold">
          <Database className="size-5 text-sidebar-primary" />
          <span>Schematic</span>
        </div>
        <nav className="flex-1 space-y-0.5 overflow-y-auto p-2">
          {NAV.map((item) => {
            const isActive =
              item.path === '/' ? pathname === '/' : pathname.startsWith(item.path)
            const Icon = item.icon
            return (
              <a
                key={item.href}
                href={item.href}
                className={cn(
                  'flex items-center gap-2 rounded-md px-3 py-2 text-sm font-medium transition-colors',
                  isActive
                    ? 'bg-sidebar-accent text-sidebar-accent-foreground'
                    : 'text-sidebar-foreground/70 hover:bg-sidebar-accent hover:text-sidebar-accent-foreground',
                )}
              >
                <Icon className="size-4" />
                {item.label}
              </a>
            )
          })}
        </nav>
      </aside>

      <div className="flex min-w-0 flex-1 flex-col">
        <header className="flex h-14 shrink-0 items-center justify-between border-b px-6">
          <div className="text-sm text-muted-foreground">
            Database schema report
          </div>
        </header>
        <main className="min-w-0 flex-1 overflow-y-auto p-6">
          <Outlet />
        </main>
      </div>
    </div>
  )
}
