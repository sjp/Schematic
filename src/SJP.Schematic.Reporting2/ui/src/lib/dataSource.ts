/**
 * Dual-mode data loader. A report must work both when opened from disk
 * (`file://`, where `fetch()` of local files is blocked) and when served over
 * `http://`. See REWRITE_PLAN "Frontend (`ui/`)".
 *
 *  - From disk: data is read from `window.__schematic`, populated by the
 *    `data/bundle.js` shim that `index.html` loads.
 *  - Over http: the canonical `.json` files are fetched lazily.
 */

declare global {
  interface Window {
    __schematic?: {
      [key: string]: unknown
    }
  }
}

const fromDisk = location.protocol === 'file:'

function bundle(): NonNullable<Window['__schematic']> {
  const data = window.__schematic
  if (data === undefined) {
    throw new Error(
      'window.__schematic is not defined — data/bundle.js failed to load (required when opening from disk).',
    )
  }
  return data
}

/** Loads a per-type summary payload (e.g. `tables`, `main`, `lint`, `search`). */
export async function loadSummary<T>(key: string): Promise<T> {
  if (fromDisk) {
    return bundle()[key] as T
  }
  const response = await fetch(`data/${key}.json`)
  if (!response.ok) {
    throw new Error(`Failed to load data/${key}.json (${response.status})`)
  }
  return (await response.json()) as T
}

/** Loads a per-object detail payload (e.g. type `table`, key `actor_a1b2c3d4`). */
export async function loadDetail<T>(type: string, key: string): Promise<T> {
  if (fromDisk) {
    const typeMap = bundle()[type] as Record<string, T> | undefined
    if (typeMap === undefined) {
      throw new Error(`No "${type}" details present in window.__schematic.`)
    }
    return typeMap[key]
  }
  const response = await fetch(`data/${type}/${key}.json`)
  if (!response.ok) {
    throw new Error(`Failed to load data/${type}/${key}.json (${response.status})`)
  }
  return (await response.json()) as T
}
