import { afterEach, beforeEach, describe, expect, it, type MockInstance, vi } from "vitest";

// `elkLayout.ts` keeps its layout engine in module state, so each test starts from a fresh import.
async function importElkLayout() {
  return (await import("@/lib/elkLayout")).layoutElkGraph;
}

const graph = () => ({
  id: "root",
  layoutOptions: { "elk.algorithm": "layered" },
  children: [
    { id: "a", width: 100, height: 50 },
    { id: "b", width: 100, height: 50 },
  ],
  edges: [{ id: "e", sources: ["a"], targets: ["b"] }],
});

type Behaviour = "answer" | "hang" | "starting" | "fail-to-start";

/** A stand-in worker that speaks ELK's message protocol without running a layout. */
class FakeWorker extends EventTarget {
  static behaviour: Behaviour = "answer";
  static instances: FakeWorker[] = [];

  onmessage: ((event: { data: unknown }) => void) | null = null;
  terminate = vi.fn<() => void>();
  layoutRequests = 0;

  constructor() {
    super();
    FakeWorker.instances.push(this);
  }

  postMessage(message: { id: number; cmd: string; graph?: { children: object[] } }) {
    const behaviour = FakeWorker.behaviour;
    queueMicrotask(() => {
      if (behaviour === "starting") return;
      if (behaviour === "fail-to-start") {
        this.dispatchEvent(new ErrorEvent("error", { message: "blocked" }));
        return;
      }
      if (message.cmd === "layout") {
        this.layoutRequests++;
        if (behaviour === "hang") return;
        const laidOut = {
          ...message.graph,
          children: message.graph!.children.map((c) => Object.assign({}, c, { x: 1, y: 2 })),
        };
        this.onmessage?.({ data: { id: message.id, data: laidOut } });
        return;
      }
      this.onmessage?.({ data: { id: message.id, data: [] } });
    });
  }

  /** Reports an uncaught error inside the worker. */
  crash(message: string) {
    this.dispatchEvent(new ErrorEvent("error", { message }));
  }
}

describe("layoutElkGraph — without Web Workers", () => {
  beforeEach(() => {
    vi.resetModules();
  });

  it("lays the graph out on the calling thread", async () => {
    expect(typeof Worker).toBe("undefined");
    const layoutElkGraph = await importElkLayout();

    const laidOut = await layoutElkGraph(graph());

    const [a, b] = laidOut.children!;
    expect(a!.x).toEqual(expect.any(Number));
    expect(b!.x).toBeGreaterThan(a!.x!);
  });

  it("rejects straight away when the signal is already aborted", async () => {
    const layoutElkGraph = await importElkLayout();
    const controller = new AbortController();
    controller.abort(new Error("gone"));

    await expect(layoutElkGraph(graph(), controller.signal)).rejects.toThrow("gone");
  });
});

describe("layoutElkGraph — with Web Workers", () => {
  let createObjectURL: MockInstance<typeof URL.createObjectURL>;

  beforeEach(() => {
    vi.resetModules();
    FakeWorker.behaviour = "answer";
    FakeWorker.instances = [];
    vi.stubGlobal("Worker", FakeWorker);
    createObjectURL = vi.spyOn(URL, "createObjectURL").mockReturnValue("blob:elk");
  });

  afterEach(() => {
    vi.unstubAllGlobals();
    vi.restoreAllMocks();
  });

  it("runs every layout in one shared worker started from a Blob URL", async () => {
    const layoutElkGraph = await importElkLayout();

    await layoutElkGraph(graph());
    const laidOut = await layoutElkGraph(graph());

    expect(laidOut.children![0]).toMatchObject({ x: 1, y: 2 });
    expect(FakeWorker.instances).toHaveLength(1);
    expect(FakeWorker.instances[0]!.layoutRequests).toBe(2);
    expect(createObjectURL).toHaveBeenCalledTimes(1);
  });

  it("falls back to the calling thread when a worker cannot be constructed", async () => {
    vi.stubGlobal("Worker", function RefusedWorker(): never {
      throw new DOMException("refused", "SecurityError");
    });
    const layoutElkGraph = await importElkLayout();

    const laidOut = await layoutElkGraph(graph());

    expect(laidOut.children![0]!.x).toEqual(expect.any(Number));
  });

  it("falls back to the calling thread when the worker fails to start", async () => {
    FakeWorker.behaviour = "fail-to-start";
    const layoutElkGraph = await importElkLayout();

    const laidOut = await layoutElkGraph(graph());

    // A real layout, rather than the stand-in worker's fixed positions.
    expect(laidOut.children![1]!.x).toBeGreaterThan(laidOut.children![0]!.x!);
    expect(FakeWorker.instances).toHaveLength(1);
    expect(FakeWorker.instances[0]!.terminate).toHaveBeenCalled();

    await layoutElkGraph(graph());
    expect(FakeWorker.instances).toHaveLength(1);
  });

  it("rejects, and starts a new worker next time, when the worker crashes during a layout", async () => {
    const layoutElkGraph = await importElkLayout();
    await layoutElkGraph(graph());

    FakeWorker.behaviour = "hang";
    const pending = layoutElkGraph(graph());
    await vi.waitFor(() => {
      expect(FakeWorker.instances[0]!.layoutRequests).toBe(2);
    });
    FakeWorker.instances[0]!.crash("out of memory");

    await expect(pending).rejects.toThrow("out of memory");

    FakeWorker.behaviour = "answer";
    await layoutElkGraph(graph());
    expect(FakeWorker.instances).toHaveLength(2);
  });

  it("stops the worker when its only layout is abandoned", async () => {
    FakeWorker.behaviour = "hang";
    const layoutElkGraph = await importElkLayout();
    const controller = new AbortController();

    const pending = layoutElkGraph(graph(), controller.signal);
    await vi.waitFor(() => {
      expect(FakeWorker.instances[0]!.layoutRequests).toBe(1);
    });
    controller.abort(new Error("superseded"));

    await expect(pending).rejects.toThrow("superseded");
    expect(FakeWorker.instances[0]!.terminate).toHaveBeenCalled();

    FakeWorker.behaviour = "answer";
    await layoutElkGraph(graph());
    expect(FakeWorker.instances).toHaveLength(2);
  });

  it("does not stop a worker that is still starting when its layout is abandoned", async () => {
    FakeWorker.behaviour = "starting";
    const layoutElkGraph = await importElkLayout();
    const controller = new AbortController();

    const pending = layoutElkGraph(graph(), controller.signal);
    controller.abort(new Error("superseded"));

    await expect(pending).rejects.toThrow("superseded");
    expect(FakeWorker.instances[0]!.terminate).not.toHaveBeenCalled();

    void layoutElkGraph(graph());
    expect(FakeWorker.instances).toHaveLength(1);
  });

  it("keeps the worker running while another layout is still waiting on it", async () => {
    FakeWorker.behaviour = "hang";
    const layoutElkGraph = await importElkLayout();
    const controller = new AbortController();

    const abandoned = layoutElkGraph(graph(), controller.signal);
    void layoutElkGraph(graph());
    await vi.waitFor(() => {
      expect(FakeWorker.instances[0]!.layoutRequests).toBe(2);
    });
    controller.abort(new Error("superseded"));

    await expect(abandoned).rejects.toThrow("superseded");
    expect(FakeWorker.instances[0]!.terminate).not.toHaveBeenCalled();
  });
});
