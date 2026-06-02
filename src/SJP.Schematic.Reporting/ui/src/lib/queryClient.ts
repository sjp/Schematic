import { QueryClient } from "@tanstack/react-query";

/**
 * A single QueryClient for the whole app. Report data is immutable for the
 * lifetime of the page (it is generated once), so caches never go stale and are
 * never garbage-collected, and there is nothing to refetch.
 */
export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      staleTime: Infinity,
      gcTime: Infinity,
      retry: false,
      refetchOnWindowFocus: false,
      refetchOnReconnect: false,
    },
  },
});
