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
import { SequencesPage } from '@/routes/sequences'
import { SequenceDetailPage } from '@/routes/sequence-detail'
import { SynonymsPage } from '@/routes/synonyms'
import { SynonymDetailPage } from '@/routes/synonym-detail'
import { TriggersPage } from '@/routes/triggers'
import { ColumnsPage } from '@/routes/columns'
import { ConstraintsPage } from '@/routes/constraints'
import { IndexesPage } from '@/routes/indexes'
import { OrphansPage } from '@/routes/orphans'
import { LintPage } from '@/routes/lint'
import { RelationshipsPage } from '@/routes/relationships'
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

const sequencesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/sequences',
  component: SequencesPage,
  loader: () => ensureSummary('sequences'),
})

const sequenceDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/sequences/$sequenceKey',
  component: SequenceDetailPage,
  loader: ({ params }) => ensureDetail('sequence', params.sequenceKey),
})

const synonymsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/synonyms',
  component: SynonymsPage,
  loader: () => ensureSummary('synonyms'),
})

const synonymDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/synonyms/$synonymKey',
  component: SynonymDetailPage,
  loader: ({ params }) => ensureDetail('synonym', params.synonymKey),
})

const triggersRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/triggers',
  component: TriggersPage,
  loader: () => ensureSummary('triggers'),
})

const columnsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/columns',
  component: ColumnsPage,
  loader: () => ensureSummary('columns'),
})

const constraintsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/constraints',
  component: ConstraintsPage,
  loader: () => ensureSummary('constraints'),
})

const indexesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/indexes',
  component: IndexesPage,
  loader: () => ensureSummary('indexes'),
})

const orphansRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/orphans',
  component: OrphansPage,
  loader: () => ensureSummary('orphans'),
})

const lintRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/lint',
  component: LintPage,
  loader: () => ensureSummary('lint'),
})

const relationshipsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: '/relationships',
  component: RelationshipsPage,
  loader: () => ensureSummary('relationships'),
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
  sequencesRoute,
  sequenceDetailRoute,
  synonymsRoute,
  synonymDetailRoute,
  triggersRoute,
  columnsRoute,
  constraintsRoute,
  indexesRoute,
  orphansRoute,
  lintRoute,
  relationshipsRoute,
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
