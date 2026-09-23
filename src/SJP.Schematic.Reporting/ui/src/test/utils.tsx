import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import {
  type AnyRouter,
  createMemoryHistory,
  createRootRoute,
  createRoute,
  createRouter,
  type RouteComponent,
  RouterProvider,
} from "@tanstack/react-router";
import { render, type RenderResult } from "@testing-library/react";
import type { ReactElement } from "react";
import { vi } from "vitest";

/** A fresh, isolated QueryClient per test — mirrors `lib/queryClient.ts`'s options. */
function createTestQueryClient() {
  return new QueryClient({
    defaultOptions: {
      queries: {
        staleTime: Infinity,
        gcTime: Infinity,
        retry: false,
        refetchOnWindowFocus: false,
        refetchOnReconnect: false,
      },
    },
  });
}

/**
 * Report payloads a test has already "loaded", keyed the way `hooks/useReportData.ts` asks for
 * them: summaries by key (`tables`, `lint`, …), details by type and then key.
 *
 * Anything not given here is fetched for real, and `test/setup.ts` stubs `fetch` to never answer,
 * so a payload left out stays pending — which is how a test shows a page's loading state.
 */
export interface ReportData {
  summaries?: Record<string, unknown>;
  details?: Record<string, Record<string, unknown>>;
}

function createSeededQueryClient({ summaries = {}, details = {} }: ReportData) {
  const queryClient = createTestQueryClient();
  for (const [key, data] of Object.entries(summaries)) {
    queryClient.setQueryData(["summary", key], data);
  }
  for (const [type, byKey] of Object.entries(details)) {
    for (const [key, data] of Object.entries(byKey)) {
      queryClient.setQueryData(["detail", type, key], data);
    }
  }
  return queryClient;
}

/**
 * Makes every payload fetch fail with `error`, for a page's error state. Undone after the test
 * (`unstubGlobals`), back to `test/setup.ts`'s never-answering fetch.
 */
export function failToLoad(error: Error) {
  vi.stubGlobal("fetch", vi.fn<typeof fetch>().mockRejectedValue(error));
}

/**
 * Renders `ui` inside a fresh `QueryClientProvider` holding `data`, so components using
 * `useSummary`/`useDetail` render it synchronously without a fetch.
 */
export function renderWithClient(ui: ReactElement, { data = {} }: { data?: ReportData } = {}) {
  const queryClient = createSeededQueryClient(data);
  return render(<QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>);
}

export interface RenderRouteOptions {
  /** The route's path, as `router.tsx` declares it — what the page's `getRouteApi` names. */
  path: string;
  /** The page the route renders. */
  component: RouteComponent;
  /** The URL to open, when it differs from `path` (e.g. has params filled in or a search). */
  url?: string;
  /** The route's search validation, for a page that reads its search through the route. */
  validateSearch?: (search: Record<string, unknown>) => object;
  /** Payloads already loaded; see {@link ReportData}. */
  data?: ReportData;
  /** A layout for the root route, rendering the page through its `<Outlet />`. */
  layout?: RouteComponent;
}

/**
 * Renders `component` as the one route of a real TanStack Router on an in-memory history, so the
 * page's `getRouteApi`, `Link` and navigation all run for real. Awaits the router's first load, so
 * the page is on screen when this resolves. Links render as the plain paths memory history uses
 * (`/tables/actor`), without the app's `#`.
 */
export async function renderRoute({
  path,
  component,
  url = path,
  validateSearch,
  data = {},
  layout,
}: RenderRouteOptions): Promise<RenderResult & { router: AnyRouter }> {
  const rootRoute = createRootRoute(layout === undefined ? {} : { component: layout });
  const pageRoute = createRoute({
    getParentRoute: () => rootRoute,
    path,
    component,
    ...(validateSearch === undefined ? {} : { validateSearch }),
  });
  const router = createRouter({
    routeTree: rootRoute.addChildren([pageRoute]),
    history: createMemoryHistory({ initialEntries: [url] }),
  });
  await router.load();

  const queryClient = createSeededQueryClient(data);
  const result = render(
    <QueryClientProvider client={queryClient}>
      <RouterProvider router={router} />
    </QueryClientProvider>,
  );
  return { ...result, router };
}
