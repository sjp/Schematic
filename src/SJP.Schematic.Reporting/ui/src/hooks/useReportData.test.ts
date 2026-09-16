import type { QueryFunctionContext, QueryKey } from "@tanstack/react-query";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  detailQueryOptions,
  ensureDetail,
  ensureSummary,
  summaryQueryOptions,
} from "@/hooks/useReportData";
import { loadDetail, loadSummary } from "@/lib/dataSource";
import { queryClient } from "@/lib/queryClient";

vi.mock("@/lib/dataSource", () => ({
  loadSummary: vi.fn<typeof loadSummary>(),
  loadDetail: vi.fn<typeof loadDetail>(),
}));

/** The context TanStack Query passes a `queryFn`; these options ignore all of it but the call. */
function queryContext<TQueryKey extends QueryKey>(
  queryKey: TQueryKey,
): QueryFunctionContext<TQueryKey> {
  return {
    client: queryClient,
    queryKey,
    signal: new AbortController().signal,
    meta: undefined,
  };
}

describe("summaryQueryOptions", () => {
  it("builds a queryKey scoped to the summary key", () => {
    expect(summaryQueryOptions("tables").queryKey).toEqual(["summary", "tables"]);
  });

  it("delegates queryFn to loadSummary with the same key", async () => {
    vi.mocked(loadSummary).mockResolvedValue({ tablesCount: 1 });
    const options = summaryQueryOptions("tables");
    await expect(options.queryFn!(queryContext(options.queryKey))).resolves.toEqual({
      tablesCount: 1,
    });
    expect(loadSummary).toHaveBeenCalledWith("tables");
  });
});

describe("detailQueryOptions", () => {
  it("builds a queryKey scoped to type and key", () => {
    expect(detailQueryOptions("table", "actor_abc").queryKey).toEqual([
      "detail",
      "table",
      "actor_abc",
    ]);
  });

  it("delegates queryFn to loadDetail with the same type/key", async () => {
    vi.mocked(loadDetail).mockResolvedValue({ name: "actor" });
    const options = detailQueryOptions("table", "actor_abc");
    await expect(options.queryFn!(queryContext(options.queryKey))).resolves.toEqual({
      name: "actor",
    });
    expect(loadDetail).toHaveBeenCalledWith("table", "actor_abc");
  });
});

describe("ensureSummary / ensureDetail", () => {
  beforeEach(() => {
    queryClient.clear();
  });

  it("ensureSummary prefetches and caches under the summary queryKey", async () => {
    vi.mocked(loadSummary).mockResolvedValue({ tablesCount: 5 });
    await expect(ensureSummary("tables")).resolves.toEqual({ tablesCount: 5 });
    expect(queryClient.getQueryData(["summary", "tables"])).toEqual({
      tablesCount: 5,
    });
  });

  it("ensureDetail prefetches and caches under the detail queryKey", async () => {
    vi.mocked(loadDetail).mockResolvedValue({ name: "actor" });
    await expect(ensureDetail("table", "actor_abc")).resolves.toEqual({
      name: "actor",
    });
    expect(queryClient.getQueryData(["detail", "table", "actor_abc"])).toEqual({
      name: "actor",
    });
  });
});
