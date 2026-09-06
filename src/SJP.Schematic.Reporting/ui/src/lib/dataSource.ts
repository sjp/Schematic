/**
 * Dual-mode data loader. A report must work both when opened from disk
 * (`file://`, where `fetch()` of local files is blocked) and when served over
 * `http://`.
 *
 *  - From disk: data is read from `window.__schematic`, populated by the
 *    `data/bundle.js` shim that `index.html` loads.
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

function bundle(): NonNullable<Window["__schematic"]> {
  const data = window.__schematic;
  if (data === undefined) {
    throw new Error(
      "window.__schematic is not defined — data/bundle.js failed to load (required when opening from disk).",
    );
  }
  return data;
}

/** Loads a per-type summary payload (e.g. `tables`, `main`, `lint`, `search`). */
export async function loadSummary<T>(key: string): Promise<T> {
  if (fromDisk) {
    return bundle()[key] as T;
  }
  const response = await fetch(`data/${key}.json`);
  if (!response.ok) {
    throw new Error(`Failed to load data/${key}.json (${response.status})`);
  }
  return (await response.json()) as T;
}

/**
 * Directory holding the `.json` files for each detail type. The generator keys the bundle by the
 * singular type name but writes the files into a plural directory, so the http path cannot be
 * derived from the bundle key; this is the one place that mapping lives.
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
  if (fromDisk) {
    const typeMap = bundle()[type] as Record<string, T | undefined> | undefined;
    if (typeMap === undefined) {
      throw new Error(`No "${type}" details present in window.__schematic.`);
    }
    const detail = typeMap[key];
    // Mirror the http path, which throws on a missing (404) detail, so an unknown/stale key never
    // resolves to a successful `undefined` that violates the Promise<T> contract.
    if (detail === undefined) {
      throw new Error(`No "${type}" detail for key "${key}" in window.__schematic.`);
    }
    return detail;
  }
  const directory = DETAIL_DIRECTORIES[type];
  if (directory === undefined) {
    throw new Error(`Unknown detail type "${type}".`);
  }
  const response = await fetch(`data/${directory}/${key}.json`);
  if (!response.ok) {
    throw new Error(`Failed to load data/${directory}/${key}.json (${response.status})`);
  }
  return (await response.json()) as T;
}
