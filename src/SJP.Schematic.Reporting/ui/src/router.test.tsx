import { describe, expect, it, vi } from "vitest";

import { ensureDetail, ensureSummary, useDetail, useSummary } from "@/hooks/useReportData";
import { router } from "@/router";

/** Every route id the tree registers. Typing the tables below with it checks the ids exist. */
type RouteId = keyof typeof router.routesById;

// The route tree is the subject here, so the prefetch helpers its loaders call are stubbed and the
// pages it mounts are never rendered.
vi.mock("@/hooks/useReportData", () => ({
  ensureSummary: vi.fn<typeof ensureSummary>(),
  ensureDetail: vi.fn<typeof ensureDetail>(),
  useSummary: vi.fn<typeof useSummary>(),
  useDetail: vi.fn<typeof useDetail>(),
}));

/** The three route options these tests read, with the argument shapes the route tree actually uses. */
interface RouteProbe {
  component?: unknown;
  loader?: (context: { params: Record<string, string> }) => unknown;
  head?: (context: { loaderData?: { name?: string; databaseName?: string } }) => {
    meta: { title: string }[];
  };
}

function routeOptions(id: RouteId): RouteProbe {
  // Only the three options above are read, and each with far less context than a live navigation
  // would supply; see `RouteProbe`.
  // oxlint-disable-next-line typescript/no-unsafe-type-assertion
  return router.routesById[id].options as RouteProbe;
}

function titleOf(id: RouteId, loaderData?: { name?: string; databaseName?: string }): string {
  return routeOptions(id).head!({ loaderData }).meta[0]!.title;
}

/** Every listing route: its path, the summary payload it prefetches and the tab title it sets. */
const SUMMARY_ROUTES: [path: RouteId, summaryKey: string, title: string][] = [
  ["/tables", "tables", "Tables"],
  ["/views", "views", "Views"],
  ["/routines", "routines", "Routines"],
  ["/sequences", "sequences", "Sequences"],
  ["/synonyms", "synonyms", "Synonyms"],
  ["/schemas", "schemas", "Schemas"],
  ["/user-defined-types", "userDefinedTypes", "User-Defined Types"],
  ["/triggers", "triggers", "Triggers"],
  ["/columns", "columns", "Columns"],
  ["/constraints", "constraints", "Constraints"],
  ["/indexes", "indexes", "Indexes"],
  ["/orphans", "orphans", "Orphans"],
  ["/lint", "lint", "Lint"],
  ["/relationships", "relationships", "Relationships"],
];

/** Every detail route: its path, the detail type it prefetches and the param naming the object. */
const DETAIL_ROUTES: [path: RouteId, detailType: string, param: string][] = [
  ["/tables/$tableKey", "table", "tableKey"],
  ["/views/$viewKey", "view", "viewKey"],
  ["/routines/$routineKey", "routine", "routineKey"],
  ["/sequences/$sequenceKey", "sequence", "sequenceKey"],
  ["/synonyms/$synonymKey", "synonym", "synonymKey"],
  ["/schemas/$schemaKey", "schema", "schemaKey"],
  ["/user-defined-types/$typeKey", "userDefinedType", "typeKey"],
];

describe("router", () => {
  it("registers every route the tree declares, and nothing else", () => {
    const expected = [
      "__root__",
      "/",
      ...SUMMARY_ROUTES.map(([path]) => path),
      ...DETAIL_ROUTES.map(([path]) => path),
    ];
    expect(Object.keys(router.routesById).toSorted()).toEqual(expected.toSorted());
  });

  it("mounts a component on every route", () => {
    for (const [id, route] of Object.entries(router.routesById)) {
      expect(route.options.component, `${id} mounts no component`).toBeDefined();
    }
  });

  it.each(SUMMARY_ROUTES)("%s prefetches the %s summary", (path, summaryKey) => {
    vi.mocked(ensureSummary).mockClear();

    routeOptions(path).loader!({ params: {} });

    expect(ensureSummary).toHaveBeenCalledExactlyOnceWith(summaryKey);
  });

  it.each(SUMMARY_ROUTES)("%s titles the tab", (path, _summaryKey, title) => {
    expect(titleOf(path)).toBe(`${title} · Schematic`);
  });

  it.each(DETAIL_ROUTES)("%s prefetches the %s named by its own param", (path, type, param) => {
    vi.mocked(ensureDetail).mockClear();

    routeOptions(path).loader!({ params: { [param]: "an-object-key" } });

    expect(ensureDetail).toHaveBeenCalledExactlyOnceWith(type, "an-object-key");
  });

  it.each(DETAIL_ROUTES)("%s titles the tab with the loaded object's name", (path) => {
    expect(titleOf(path, { name: "film_actor" })).toBe("film_actor · Schematic");
  });

  it.each(DETAIL_ROUTES)("%s falls back to the bare brand before its data resolves", (path) => {
    expect(titleOf(path)).toBe("Schematic");
    expect(titleOf(path, { name: "" })).toBe("Schematic");
  });

  it("prefetches the main summary for the dashboard and titles the tab with the database", () => {
    vi.mocked(ensureSummary).mockClear();

    routeOptions("/").loader!({ params: {} });

    expect(ensureSummary).toHaveBeenCalledExactlyOnceWith("main");
    expect(titleOf("/", { databaseName: "sakila" })).toBe("sakila · Schematic");
    expect(titleOf("/")).toBe("Schematic");
  });

  it("titles an unmatched route with the bare brand, and gives it a not-found component", () => {
    expect(titleOf("__root__")).toBe("Schematic");
    expect(router.routesById.__root__.options.notFoundComponent).toBeDefined();
  });

  it("keeps the lint page's url selection validated", () => {
    expect(router.routesById["/lint"].options.validateSearch).toBeDefined();
  });

  it("uses hash history so deep links survive being opened from a file", () => {
    // The report is opened by double-clicking `index.html`, where there is no server to resolve
    // clean URLs, so every in-app link has to live in the fragment.
    expect(router.history.createHref("/tables")).toBe("/#/tables");
  });
});
