import type { UseQueryResult } from "@tanstack/react-query";

/**
 * The fields a page reads off a `useQuery` result. The real `UseQueryResult` is a union of a
 * dozen-odd fields describing fetch bookkeeping no page here looks at, so a test states the four
 * that decide what gets rendered and nothing else.
 */
interface RenderedQueryState<T> {
  isPending: boolean;
  isError: boolean;
  data: T | undefined;
  error: Error | null;
}

function asQueryResult<T>(state: RenderedQueryState<T>): UseQueryResult<T> {
  // Only the four fields above are ever read; see `RenderedQueryState`.
  // oxlint-disable-next-line typescript/no-unsafe-type-assertion
  return state as UseQueryResult<T>;
}

/** A query that has not answered yet. */
export function pendingQuery<T>(): UseQueryResult<T> {
  return asQueryResult<T>({ isPending: true, isError: false, data: undefined, error: null });
}

/** A query that failed. */
export function failedQuery<T>(error: Error): UseQueryResult<T> {
  return asQueryResult<T>({ isPending: false, isError: true, data: undefined, error });
}

/** A query that answered with `data`. */
export function loadedQuery<T>(data: T): UseQueryResult<T> {
  return asQueryResult<T>({ isPending: false, isError: false, data, error: null });
}
