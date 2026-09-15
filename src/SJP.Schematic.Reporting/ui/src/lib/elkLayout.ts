import ELK, { type ElkNode } from "elkjs/lib/elk-api.js";
// The worker script is inlined as text and started from a Blob URL. A report opened from disk
// (`file://`) cannot start a worker from a script file, and the app is a single classic script, so
// there is no separate worker chunk to point at either.
import elkWorkerSource from "elkjs/lib/elk-worker.min.js?raw";

/**
 * Runs ELK graph layouts without blocking the page. Layouts run in a shared Web Worker. When a
 * worker cannot be started (no `Worker`, or the browser refuses it), layouts fall back to running
 * ELK on the calling thread.
 */

type LayoutEngine = {
  elk: InstanceType<typeof ELK>;
  /** Layouts currently waiting on this engine. */
  running: number;
  /** Rejects once the engine can no longer run layouts. Never settles for a healthy engine. */
  failed: Promise<never>;
  /** The error `failed` rejected with, once it has. */
  failure?: Error;
  /**
   * Stops the engine's worker, if it can be stopped safely. `undefined` for the calling-thread
   * engine, which cannot be stopped.
   */
  terminate?: () => boolean;
};

/** The engine failed before it ever answered, i.e. its worker could not start. */
class WorkerStartError extends Error {}

let workerUrl: string | undefined;
let current: LayoutEngine | undefined;
let workersUnavailable = false;

function createWorkerEngine(): LayoutEngine {
  // One Blob URL serves every worker the page starts, so it is never revoked: a replacement worker is
  // started from it after an abandoned or failed layout.
  workerUrl ??= URL.createObjectURL(new Blob([elkWorkerSource], { type: "text/javascript" }));

  let worker: Worker | undefined;
  let answered = false;
  let reject: (error: Error) => void = () => {};
  const failed = new Promise<never>((_, rejectFailed) => {
    reject = rejectFailed;
  });
  // Nothing may be waiting on the engine when it fails.
  failed.catch(() => {});

  const elk = new ELK({
    workerFactory: () => {
      worker = new Worker(workerUrl!);
      worker.addEventListener("error", (event) => {
        if (engine.failure !== undefined) return;
        const message = event.message || "The diagram layout worker failed.";
        engine.failure = answered ? new Error(message) : new WorkerStartError(message);
        worker?.terminate();
        reject(engine.failure);
      });
      return worker;
    },
  });
  const engine: LayoutEngine = {
    elk,
    running: 0,
    failed,
    // Firefox can crash the page when a worker is stopped while it is still evaluating its script,
    // so a worker is only stopped once it has answered.
    terminate: () => {
      if (!answered) return false;
      worker?.terminate();
      return true;
    },
  };

  // A cheap request answered ahead of any layout, so a failure before it is answered is a failure
  // to start rather than a layout that crashed the worker.
  void elk.knownLayoutCategories().then(
    () => {
      answered = true;
    },
    () => {},
  );

  return engine;
}

function createCallingThreadEngine(): LayoutEngine {
  // The worker script doubles as a CommonJS module that exports an in-process stand-in for a
  // worker, which is how ELK runs without one.
  const module: { exports: { Worker?: new () => Worker } } = { exports: {} };
  new Function("module", "exports", elkWorkerSource)(module, module.exports);
  const FakeWorker = module.exports.Worker!;
  const elk = new ELK({ workerFactory: () => new FakeWorker() });
  return { elk, running: 0, failed: new Promise<never>(() => {}) };
}

function getEngine(): LayoutEngine {
  if (current === undefined) {
    if (!workersUnavailable && typeof Worker !== "undefined") {
      try {
        current = createWorkerEngine();
        return current;
      } catch {
        workersUnavailable = true;
      }
    }
    current = createCallingThreadEngine();
  }
  return current;
}

function whenAborted(signal: AbortSignal | undefined): Promise<never> {
  return new Promise((_, reject) => {
    if (signal === undefined) return;
    if (signal.aborted) {
      reject(signal.reason);
      return;
    }
    signal.addEventListener("abort", () => reject(signal.reason), { once: true });
  });
}

/**
 * Lays out `graph` with ELK, resolving to the graph with positions and sizes filled in.
 *
 * Aborting `signal` rejects with the signal's reason. If no other layout is waiting on the worker and
 * the worker has finished starting, the worker is also stopped, so an abandoned layout of a large
 * graph does not hold up the next one.
 */
export async function layoutElkGraph<T extends ElkNode>(graph: T, signal?: AbortSignal) {
  signal?.throwIfAborted();

  const engine = getEngine();
  engine.running++;
  try {
    return await Promise.race([engine.elk.layout(graph), engine.failed, whenAborted(signal)]);
  } catch (error) {
    if (signal?.aborted || error !== engine.failure) {
      // Abandoned, or ELK itself rejected the graph.
      throw error;
    }
    // Replace the failed engine. A worker that never started will not start next time either, so
    // later layouts run on the calling thread. A worker that crashed part way through a layout is
    // not retried here: the same graph would most likely crash the page as well.
    if (current === engine) current = undefined;
    if (error instanceof WorkerStartError) {
      workersUnavailable = true;
      return layoutElkGraph(graph, signal);
    }
    throw error;
  } finally {
    engine.running--;
    if (signal?.aborted && engine.running === 0 && engine.terminate?.() && current === engine) {
      current = undefined;
    }
  }
}
