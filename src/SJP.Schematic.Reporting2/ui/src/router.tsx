import {
  createHashHistory,
  createRootRoute,
  createRoute,
  createRouter,
} from '@tanstack/react-router'
import { RootLayout } from '@/components/layout/RootLayout'
import { NotFound } from '@/components/layout/NotFound'
import { DashboardPage } from '@/routes/dashboard'
import { TablesPage } from '@/routes/tables'
import { TableDetailPage } from '@/routes/table-detail'
import { ensureDetail, ensureSummary } from '@/hooks/useReportData'

const rootRoute = createRootRoute({
  component: RootLayout,
  notFoundComponent: NotFound,
})

const dashboardRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/',
  component: DashboardPage,
  loader: () => ensureSummary('main'),
})

const tablesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/tables',
  component: TablesPage,
  loader: () => ensureSummary('tables'),
})

const tableDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/tables/$tableKey',
  component: TableDetailPage,
  loader: ({ params }) => ensureDetail('table', params.tableKey),
})

// Child routes for each object type are registered here by later waves.
const routeTree = rootRoute.addChildren([
  dashboardRoute,
  tablesRoute,
  tableDetailRoute,
])

// Hash history is mandatory so deep links survive `file://` (no server to
// resolve clean URLs).
export const router = createRouter({
  routeTree,
  history: createHashHistory(),
})

declare module '@tanstack/react-router' {
  interface Register {
    router: typeof router
  }
}
