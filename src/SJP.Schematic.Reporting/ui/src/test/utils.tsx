import type { ReactElement } from "react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { render } from "@testing-library/react";

/** A fresh, isolated QueryClient per test — mirrors `lib/queryClient.ts`'s options. */
function createTestQueryClient() {
  return new QueryClient({
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
}

/**
 * Renders `ui` inside a fresh `QueryClientProvider`, optionally pre-seeding query
 * data so components using `useSummary`/`useDetail` render synchronously without
 * a real fetch. `seed` entries use the same query-key shape as
 * `hooks/useReportData.ts` (`["summary", key]` / `["detail", type, key]`).
 */
export function renderWithClient(
  ui: ReactElement,
  {
    seed,
  }: { seed?: Array<{ queryKey: readonly unknown[]; data: unknown }> } = {},
) {
  const queryClient = createTestQueryClient();
  for (const { queryKey, data } of seed ?? []) {
    queryClient.setQueryData(queryKey, data);
  }

  return render(
    <QueryClientProvider client={queryClient}>{ui}</QueryClientProvider>,
  );
}
