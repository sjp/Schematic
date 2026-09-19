import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { QueryFunctionContext, QueryKey } from "@tanstack/react-query";
import { renderHook, waitFor } from "@testing-library/react";
import { createElement, type ReactNode } from "react";
import { beforeEach, describe, expect, it, vi } from "vitest";

import {
  detailQueryOptions,
  ensureDetail,
  ensureSummary,
  summaryQueryOptions,
  useDetail,
  useSummary,
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

/** A fresh client per render, so one test's cache never answers another's query. */
function wrapper({ children }: { children: ReactNode }) {
  const client = new QueryClient({
    defaultOptions: { queries: { retry: false, gcTime: Infinity, staleTime: Infinity } },
  });
  return createElement(QueryClientProvider, { client }, children);
}

describe("useSummary / useDetail", () => {
  it("useSummary reads the payload for its key", async () => {
    vi.mocked(loadSummary).mockResolvedValue({ tablesCount: 3 });

    const { result } = renderHook(() => useSummary<{ tablesCount: number }>("tables"), { wrapper });

    await waitFor(() => {
      expect(result.current.data).toEqual({ tablesCount: 3 });
    });
    expect(loadSummary).toHaveBeenCalledWith("tables");
  });

  it("useDetail reads the payload for its type and key", async () => {
    vi.mocked(loadDetail).mockResolvedValue({ name: "actor" });

    const { result } = renderHook(() => useDetail<{ name: string }>("table", "actor_abc"), {
      wrapper,
    });

    await waitFor(() => {
      expect(result.current.data).toEqual({ name: "actor" });
    });
    expect(loadDetail).toHaveBeenCalledWith("table", "actor_abc");
  });

  it("surfaces a payload that fails to load", async () => {
    vi.mocked(loadDetail).mockRejectedValue(new Error("404"));

    const { result } = renderHook(() => useDetail("table", "missing"), { wrapper });

    await waitFor(() => {
      expect(result.current.isError).toBe(true);
    });
    expect(result.current.error).toEqual(new Error("404"));
  });
});
