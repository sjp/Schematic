import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

// `dataSource.ts` decides its mode from `location.protocol` once, at module-evaluation
// time, so each scenario stubs `location` *before* a fresh dynamic import.
function stubProtocol(protocol: "file:" | "http:") {
  vi.stubGlobal("location", { ...window.location, protocol });
}

describe("dataSource — served over http", () => {
  beforeEach(() => {
    vi.resetModules();
    stubProtocol("http:");
    vi.stubGlobal("fetch", vi.fn());
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("loadSummary fetches and parses the summary json", async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ tablesCount: 2 }),
    } as Response);

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).resolves.toEqual({ tablesCount: 2 });
    expect(fetch).toHaveBeenCalledWith("data/tables.json");
  });

  it("loadSummary throws when the response is not ok", async () => {
    vi.mocked(fetch).mockResolvedValue({ ok: false, status: 404 } as Response);

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).rejects.toThrow(
      "Failed to load data/tables.json (404)",
    );
  });

  it("loadDetail fetches and parses the detail json", async () => {
    vi.mocked(fetch).mockResolvedValue({
      ok: true,
      json: () => Promise.resolve({ name: "actor" }),
    } as Response);

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "actor_abc123")).resolves.toEqual({
      name: "actor",
    });
    expect(fetch).toHaveBeenCalledWith("data/table/actor_abc123.json");
  });

  it("loadDetail throws when the response is not ok", async () => {
    vi.mocked(fetch).mockResolvedValue({ ok: false, status: 404 } as Response);

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "missing")).rejects.toThrow(
      "Failed to load data/table/missing.json (404)",
    );
  });
});

describe("dataSource — opened from disk", () => {
  beforeEach(() => {
    vi.resetModules();
    stubProtocol("file:");
    delete window.__schematic;
  });

  afterEach(() => {
    vi.unstubAllGlobals();
  });

  it("loadSummary reads the key straight off window.__schematic", async () => {
    window.__schematic = { tables: { tablesCount: 3 } };

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).resolves.toEqual({ tablesCount: 3 });
  });

  it("loadSummary throws when window.__schematic is not defined", async () => {
    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).rejects.toThrow(
      "window.__schematic is not defined",
    );
  });

  it("loadDetail reads the nested type/key entry", async () => {
    window.__schematic = { table: { actor_abc123: { name: "actor" } } };

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "actor_abc123")).resolves.toEqual({
      name: "actor",
    });
  });

  it("loadDetail throws when the type map is missing", async () => {
    window.__schematic = {};

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "actor_abc123")).rejects.toThrow(
      'No "table" details present in window.__schematic.',
    );
  });

  it("loadDetail throws when the key is missing from the type map", async () => {
    window.__schematic = { table: {} };

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "missing")).rejects.toThrow(
      'No "table" detail for key "missing" in window.__schematic.',
    );
  });
});
