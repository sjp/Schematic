import { beforeEach, describe, expect, it, vi } from "vitest";
import { loadDetail, loadSummary } from "@/lib/dataSource";
import {
  detailQueryOptions,
  ensureDetail,
  ensureSummary,
  summaryQueryOptions,
} from "@/hooks/useReportData";
import { queryClient } from "@/lib/queryClient";

vi.mock("@/lib/dataSource", () => ({
  loadSummary: vi.fn(),
  loadDetail: vi.fn(),
}));

describe("summaryQueryOptions", () => {
  it("builds a queryKey scoped to the summary key", () => {
    expect(summaryQueryOptions("tables").queryKey).toEqual([
      "summary",
      "tables",
    ]);
  });

  it("delegates queryFn to loadSummary with the same key", async () => {
    vi.mocked(loadSummary).mockResolvedValue({ tablesCount: 1 });
    const options = summaryQueryOptions("tables");
    await expect(options.queryFn!({} as never)).resolves.toEqual({
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
    await expect(options.queryFn!({} as never)).resolves.toEqual({
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
