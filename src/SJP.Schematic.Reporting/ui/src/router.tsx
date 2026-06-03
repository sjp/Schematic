import {
  createHashHistory,
  createRootRoute,
  createRoute,
  createRouter,
} from "@tanstack/react-router";
import { RootLayout } from "@/components/layout/RootLayout";
import { NotFound } from "@/components/layout/NotFound";
import { DashboardPage } from "@/routes/dashboard";
import { TablesPage } from "@/routes/tables";
import { TableDetailPage } from "@/routes/table-detail";
import { ViewsPage } from "@/routes/views";
import { ViewDetailPage } from "@/routes/view-detail";
import { RoutinesPage } from "@/routes/routines";
import { RoutineDetailPage } from "@/routes/routine-detail";
import { SequencesPage } from "@/routes/sequences";
import { SequenceDetailPage } from "@/routes/sequence-detail";
import { SynonymsPage } from "@/routes/synonyms";
import { SynonymDetailPage } from "@/routes/synonym-detail";
import { TriggersPage } from "@/routes/triggers";
import { ColumnsPage } from "@/routes/columns";
import { ConstraintsPage } from "@/routes/constraints";
import { IndexesPage } from "@/routes/indexes";
import { OrphansPage } from "@/routes/orphans";
import { LintPage } from "@/routes/lint";
import { RelationshipsPage } from "@/routes/relationships";
import { ensureDetail, ensureSummary } from "@/hooks/useReportData";
import type {
  MainSummary,
  RoutineDetail,
  SequenceDetail,
  SynonymDetail,
  TableDetail,
  ViewDetail,
} from "@/types/report";

const BRAND = "Schematic";
/** Browser-tab title for a page, e.g. `pageTitle("Lint")` → "Lint · Schematic". */
const pageTitle = (name: string) => `${name} · ${BRAND}`;
// Falls back to the bare brand if no name is available (e.g. loader data not yet
// resolved), avoiding a "· Schematic" title with an empty leading segment.
const titleMeta = (name?: string) => ({
  meta: [{ title: name ? pageTitle(name) : BRAND }],
});

const rootRoute = createRootRoute({
  component: RootLayout,
  notFoundComponent: NotFound,
  // Default title for unmatched routes (NotFound); child routes override it.
  head: () => ({ meta: [{ title: BRAND }] }),
});

const dashboardRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/",
  component: DashboardPage,
  loader: () => ensureSummary<MainSummary>("main"),
  head: ({ loaderData }) => titleMeta(loaderData?.databaseName),
});

const tablesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/tables",
  component: TablesPage,
  loader: () => ensureSummary("tables"),
  head: () => titleMeta("Tables"),
});

const tableDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/tables/$tableKey",
  component: TableDetailPage,
  loader: ({ params }) => ensureDetail<TableDetail>("table", params.tableKey),
  head: ({ loaderData }) => titleMeta(loaderData?.name),
});

const viewsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/views",
  component: ViewsPage,
  loader: () => ensureSummary("views"),
  head: () => titleMeta("Views"),
});

const viewDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/views/$viewKey",
  component: ViewDetailPage,
  loader: ({ params }) => ensureDetail<ViewDetail>("view", params.viewKey),
  head: ({ loaderData }) => titleMeta(loaderData?.name),
});

const routinesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/routines",
  component: RoutinesPage,
  loader: () => ensureSummary("routines"),
  head: () => titleMeta("Routines"),
});

const routineDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/routines/$routineKey",
  component: RoutineDetailPage,
  loader: ({ params }) =>
    ensureDetail<RoutineDetail>("routine", params.routineKey),
  head: ({ loaderData }) => titleMeta(loaderData?.name),
});

const sequencesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/sequences",
  component: SequencesPage,
  loader: () => ensureSummary("sequences"),
  head: () => titleMeta("Sequences"),
});

const sequenceDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/sequences/$sequenceKey",
  component: SequenceDetailPage,
  loader: ({ params }) =>
    ensureDetail<SequenceDetail>("sequence", params.sequenceKey),
  head: ({ loaderData }) => titleMeta(loaderData?.name),
});

const synonymsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/synonyms",
  component: SynonymsPage,
  loader: () => ensureSummary("synonyms"),
  head: () => titleMeta("Synonyms"),
});

const synonymDetailRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/synonyms/$synonymKey",
  component: SynonymDetailPage,
  loader: ({ params }) =>
    ensureDetail<SynonymDetail>("synonym", params.synonymKey),
  head: ({ loaderData }) => titleMeta(loaderData?.name),
});

const triggersRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/triggers",
  component: TriggersPage,
  loader: () => ensureSummary("triggers"),
  head: () => titleMeta("Triggers"),
});

const columnsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/columns",
  component: ColumnsPage,
  loader: () => ensureSummary("columns"),
  head: () => titleMeta("Columns"),
});

const constraintsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/constraints",
  component: ConstraintsPage,
  loader: () => ensureSummary("constraints"),
  head: () => titleMeta("Constraints"),
});

const indexesRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/indexes",
  component: IndexesPage,
  loader: () => ensureSummary("indexes"),
  head: () => titleMeta("Indexes"),
});

const orphansRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/orphans",
  component: OrphansPage,
  loader: () => ensureSummary("orphans"),
  head: () => titleMeta("Orphans"),
});

const lintRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/lint",
  component: LintPage,
  loader: () => ensureSummary("lint"),
  head: () => titleMeta("Lint"),
});

const relationshipsRoute = createRoute({
  getParentRoute: () => rootRoute,
  path: "/relationships",
  component: RelationshipsPage,
  loader: () => ensureSummary("relationships"),
  head: () => titleMeta("Relationships"),
});

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
]);

// Hash history is mandatory so deep links survive `file://` (no server to
// resolve clean URLs).
export const router = createRouter({
  routeTree,
  history: createHashHistory(),
});

declare module "@tanstack/react-router" {
  interface Register {
    router: typeof router;
  }
}
