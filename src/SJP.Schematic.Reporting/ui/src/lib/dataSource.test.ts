import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";

// `dataSource.ts` decides its mode from `location.protocol` once, at module-evaluation
// time, so each scenario stubs `location` *before* a fresh dynamic import.
function stubProtocol(protocol: "file:" | "http:") {
  vi.stubGlobal("location", { href: window.location.href, protocol });
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
    vi.mocked(fetch).mockResolvedValue(Response.json({ tablesCount: 2 }));

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).resolves.toEqual({ tablesCount: 2 });
    expect(fetch).toHaveBeenCalledWith("data/tables.json");
  });

  it("loadSummary throws when the response is not ok", async () => {
    vi.mocked(fetch).mockResolvedValue(new Response(null, { status: 404 }));

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).rejects.toThrow("Failed to load data/tables.json (404)");
  });

  it("loadDetail fetches and parses the detail json", async () => {
    vi.mocked(fetch).mockResolvedValue(Response.json({ name: "actor" }));

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "actor_abc123")).resolves.toEqual({
      name: "actor",
    });
    expect(fetch).toHaveBeenCalledWith("data/tables/actor_abc123.json");
  });

  it("loadDetail throws when the response is not ok", async () => {
    vi.mocked(fetch).mockResolvedValue(new Response(null, { status: 404 }));

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "missing")).rejects.toThrow(
      "Failed to load data/tables/missing.json (404)",
    );
  });

  it("loadDetail throws for a detail type it has no directory for", async () => {
    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("nonsense", "key")).rejects.toThrow('Unknown detail type "nonsense".');
    expect(fetch).not.toHaveBeenCalled();
  });
});

type ScriptResponse = (src: string) => "error" | (() => void);

/**
 * jsdom does not load scripts, so this stands in for the browser: each script the loader adds is
 * answered by `respond`, which either fails it or returns what running it would do.
 */
function answerScripts(respond: ScriptResponse) {
  const added: HTMLScriptElement[] = [];
  vi.spyOn(document.head, "append").mockImplementation((...nodes) => {
    for (const node of nodes) {
      if (!(node instanceof HTMLScriptElement)) {
        continue;
      }
      const script = node;
      added.push(script);
      const src = script.getAttribute("src")!;
      setTimeout(() => {
        const response = respond(src);
        if (response === "error") {
          script.dispatchEvent(new Event("error"));
        } else {
          response();
          script.dispatchEvent(new Event("load"));
        }
      });
    }
  });
  return added;
}

describe("dataSource — opened from disk", () => {
  beforeEach(() => {
    vi.resetModules();
    stubProtocol("file:");
    delete window.__schematic;
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("loadSummary runs the summary's script and returns what it assigned", async () => {
    const scripts = answerScripts(() => () => {
      window.__schematic = { ...window.__schematic, tables: { tablesCount: 3 } };
    });

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).resolves.toEqual({ tablesCount: 3 });
    expect(scripts.map((s) => s.getAttribute("src"))).toEqual(["data/bundle/tables.js"]);
  });

  it("loadSummary takes the payload off window.__schematic once it is loaded", async () => {
    answerScripts(() => () => {
      window.__schematic = { tables: { tablesCount: 3 }, other: 1 };
    });

    const { loadSummary } = await import("@/lib/dataSource");
    await loadSummary("tables");
    expect(window.__schematic).toEqual({ other: 1 });
  });

  it("loadSummary throws when the script fails to load", async () => {
    answerScripts(() => "error");

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).rejects.toThrow("Failed to load data/bundle/tables.js");
  });

  it("loadSummary throws when the script does not define the payload", async () => {
    answerScripts(() => () => {});

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).rejects.toThrow(
      "data/bundle/tables.js loaded but did not define its data.",
    );
  });

  it("concurrent loads of the same payload share one script", async () => {
    const scripts = answerScripts(() => () => {
      window.__schematic = { tables: { tablesCount: 3 } };
    });

    const { loadSummary } = await import("@/lib/dataSource");
    const [first, second] = await Promise.all([loadSummary("tables"), loadSummary("tables")]);

    expect(first).toEqual({ tablesCount: 3 });
    expect(second).toBe(first);
    expect(scripts).toHaveLength(1);
  });

  it("loads the payload again after an earlier load has finished", async () => {
    let runs = 0;
    const scripts = answerScripts(() => () => {
      runs++;
      window.__schematic = { tables: { run: runs } };
    });

    const { loadSummary } = await import("@/lib/dataSource");
    await expect(loadSummary("tables")).resolves.toEqual({ run: 1 });
    await expect(loadSummary("tables")).resolves.toEqual({ run: 2 });
    expect(scripts).toHaveLength(2);
  });

  it("removes each script element once it has run", async () => {
    vi.restoreAllMocks();
    const { loadSummary } = await import("@/lib/dataSource");

    const loading = loadSummary("tables");
    const script = document.head.querySelector<HTMLScriptElement>(
      'script[src="data/bundle/tables.js"]',
    )!;
    window.__schematic = { tables: [] };
    script.dispatchEvent(new Event("load"));

    await expect(loading).resolves.toEqual([]);
    expect(script.isConnected).toBe(false);
  });

  it("loadDetail runs the object's script from its type directory", async () => {
    const scripts = answerScripts(() => () => {
      window.__schematic = { table: { other_key: {}, actor_abc123: { name: "actor" } } };
    });

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "actor_abc123")).resolves.toEqual({ name: "actor" });
    expect(scripts.map((s) => s.getAttribute("src"))).toEqual([
      "data/bundle/table/actor_abc123.js",
    ]);
    expect(window.__schematic).toEqual({ table: { other_key: {} } });
  });

  it("loadDetail throws when the script does not define the payload", async () => {
    answerScripts(() => () => {
      window.__schematic = { table: {} };
    });

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("table", "missing")).rejects.toThrow(
      "data/bundle/table/missing.js loaded but did not define its data.",
    );
  });

  it("loadDetail throws for a detail type it does not know", async () => {
    const scripts = answerScripts(() => "error");

    const { loadDetail } = await import("@/lib/dataSource");
    await expect(loadDetail("nonsense", "key")).rejects.toThrow('Unknown detail type "nonsense".');
    expect(scripts).toHaveLength(0);
  });
});
