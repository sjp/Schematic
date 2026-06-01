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
import { ViewsPage } from '@/routes/views'
import { ViewDetailPage } from '@/routes/view-detail'
import { RoutinesPage } from '@/routes/routines'
import { RoutineDetailPage } from '@/routes/routine-detail'
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

const viewsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/views',
  component: ViewsPage,
  loader: () => ensureSummary('views'),
})

const viewDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/views/$viewKey',
  component: ViewDetailPage,
  loader: ({ params }) => ensureDetail('view', params.viewKey),
})

const routinesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/routines',
  component: RoutinesPage,
  loader: () => ensureSummary('routines'),
})

const routineDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/routines/$routineKey',
  component: RoutineDetailPage,
  loader: ({ params }) => ensureDetail('routine', params.routineKey),
})

// Child routes for each object type are registered here by later waves.
const routeTree = rootRoute.addChildren([
  dashboardRoute,
  tablesRoute,
  tableDetailRoute,
  viewsRoute,
  viewDetailRoute,
  routinesRoute,
  routineDetailRoute,
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
