import { queryOptions, useQuery } from "@tanstack/react-query";
import { loadDetail, loadSummary } from "@/lib/dataSource";
import { queryClient } from "@/lib/queryClient";

/** Query options for a per-type summary payload. Shared by hooks and route loaders. */
export function summaryQueryOptions<T>(key: string) {
  return queryOptions({
    queryKey: ["summary", key] as const,
    queryFn: () => loadSummary<T>(key),
  });
}

/** Query options for a per-object detail payload. Shared by hooks and route loaders. */
export function detailQueryOptions<T>(type: string, key: string) {
  return queryOptions({
    queryKey: ["detail", type, key] as const,
    queryFn: () => loadDetail<T>(type, key),
  });
}

/** Reads a per-type summary payload (e.g. `tables`, `main`, `lint`). */
export function useSummary<T>(key: string) {
  return useQuery(summaryQueryOptions<T>(key));
}

/** Reads a per-object detail payload (e.g. type `table`, key `actor_a1b2c3d4`). */
export function useDetail<T>(type: string, key: string) {
  return useQuery(detailQueryOptions<T>(type, key));
}

/** Prefetch helper so TanStack Router route loaders can `ensureQueryData`. */
export function ensureSummary<T>(key: string) {
  return queryClient.ensureQueryData(summaryQueryOptions<T>(key));
}

/** Prefetch helper so TanStack Router route loaders can `ensureQueryData`. */
export function ensureDetail<T>(type: string, key: string) {
  return queryClient.ensureQueryData(detailQueryOptions<T>(type, key));
}
