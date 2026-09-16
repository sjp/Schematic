/**
 * Dual-mode data loader. A report must work both when opened from disk
 * (`file://`, where `fetch()` of local files is blocked) and when served over
 * `http://`.
 *
 *  - From disk: each payload has its own classic script under `data/bundle/`,
 *    which assigns the payload onto `window.__schematic`. A script is added to
 *    the page only when its payload is first needed, since script elements load
 *    from disk where `fetch()` cannot.
 *  - Over http: the canonical `.json` files are fetched lazily.
 */

declare global {
  interface Window {
    __schematic?: {
      [key: string]: unknown;
    };
  }
}

const fromDisk = location.protocol === "file:";

/** Narrows a payload entry to an object, so a bundle that defined something else reads as absent. */
function isRecord(value: unknown): value is Record<string, unknown> {
  return typeof value === "object" && value !== null;
}

/**
 * Reads a fetched payload as `T`. Payloads are generated alongside this app from the same schema
 * and are never validated at runtime, so this is the one place that trust is taken.
 */
async function readJson<T>(response: Response): Promise<T> {
  // oxlint-disable-next-line typescript/no-unsafe-type-assertion
  return (await response.json()) as T;
}

// Payloads being loaded from disk, by script path. Concurrent requests for the same payload share
// one script, because the first to finish takes the payload off `window.__schematic`.
const pendingScripts = new Map<string, Promise<unknown>>();

/**
 * Runs the script at `src`, then takes the payload it defined off `window.__schematic` with `take`,
 * so payloads do not pile up there as the reader moves between pages.
 */
function loadFromScript<T>(
  src: string,
  take: (data: Record<string, unknown>) => unknown,
): Promise<T> {
  let pending = pendingScripts.get(src);
  if (pending === undefined) {
    pending = new Promise<unknown>((resolve, reject) => {
      const script = document.createElement("script");
      script.src = src;
      script.addEventListener("load", () => {
        script.remove();
        const payload = take((window.__schematic ??= {}));
        if (payload === undefined) {
          reject(new Error(`${src} loaded but did not define its data.`));
        } else {
          resolve(payload);
        }
      });
      script.addEventListener("error", () => {
        script.remove();
        reject(new Error(`Failed to load ${src}`));
      });
      document.head.append(script);
    }).finally(() => pendingScripts.delete(src));
    pendingScripts.set(src, pending);
  }
  // The payload a bundle script defines is whatever the generator wrote for this `src`; see
  // `readJson` for the same trust taken on the fetched path.
  // oxlint-disable-next-line typescript/no-unsafe-type-assertion
  return pending as Promise<T>;
}

/** Loads a per-type summary payload (e.g. `tables`, `main`, `lint`, `search`). */
export async function loadSummary<T>(key: string): Promise<T> {
  if (fromDisk) {
    return loadFromScript<T>(`data/bundle/${key}.js`, (data) => {
      const payload = data[key];
      delete data[key];
      return payload;
    });
  }
  const response = await fetch(`data/${key}.json`);
  if (!response.ok) {
    throw new Error(`Failed to load data/${key}.json (${response.status})`);
  }
  return readJson<T>(response);
}

/**
 * Directory holding the `.json` files for each detail type. The generator keys detail payloads by
 * the singular type name but writes the files into a plural directory, so the http path cannot be
 * derived from the key; this is the one place that mapping lives.
 */
const DETAIL_DIRECTORIES: Record<string, string> = {
  table: "tables",
  view: "views",
  sequence: "sequences",
  synonym: "synonyms",
  routine: "routines",
  schema: "schemas",
  userDefinedType: "userDefinedTypes",
};

/** Loads a per-object detail payload (e.g. type `table`, key `actor_a1b2c3d4`). */
export async function loadDetail<T>(type: string, key: string): Promise<T> {
  const directory = DETAIL_DIRECTORIES[type];
  if (directory === undefined) {
    throw new Error(`Unknown detail type "${type}".`);
  }
  if (fromDisk) {
    return loadFromScript<T>(`data/bundle/${type}/${key}.js`, (data) => {
      const entry = data[type];
      const typeMap = isRecord(entry) ? entry : undefined;
      const payload = typeMap?.[key];
      delete typeMap?.[key];
      return payload;
    });
  }
  const response = await fetch(`data/${directory}/${key}.json`);
  if (!response.ok) {
    throw new Error(`Failed to load data/${directory}/${key}.json (${response.status})`);
  }
  return readJson<T>(response);
}
