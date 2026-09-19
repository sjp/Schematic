import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { RoutinesPage } from "@/routes/routines";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { RoutinesSummary, RoutineSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

// `routines.tsx` only imports `Link` from this package; stub it as a plain anchor so the route can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  Link: ({
    to,
    params,
    children,
    className,
  }: {
    to: string;
    params: Record<string, string>;
    children: React.ReactNode;
    className?: string;
  }) => {
    const href = Object.entries(params).reduce(
      (path, [key, value]) => path.replace(`$${key}`, value),
      to,
    );
    return (
      <a href={href} className={className}>
        {children}
      </a>
    );
  },
}));

const mockUseSummary = vi.mocked(useSummary<RoutinesSummary>);

const ROUTINE: RoutineSummary = {
  name: "film_in_stock",
  routineUrl: "#/routines/film_in_stock-1a2b3c4d",
  routineType: "Procedure",
};

describe("RoutinesPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<RoutinesPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("boom")));

    render(<RoutinesPage />);
    expect(screen.getByText("Failed to load routines: boom")).toBeInTheDocument();
  });

  it("links each row's name to its routine detail route, derived from the hash url", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ routinesCount: 1, allRoutines: [ROUTINE] }));

    render(<RoutinesPage />);
    expect(screen.getByRole("link", { name: "film_in_stock" })).toHaveAttribute(
      "href",
      "/routines/film_in_stock-1a2b3c4d",
    );
  });

  it("shows a known kind but leaves the cell blank for an unrecorded one", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        routinesCount: 2,
        allRoutines: [
          ROUTINE,
          {
            name: "get_customer_balance",
            routineUrl: "#/routines/get_customer_balance-5e6f",
            routineType: "Unknown",
          },
        ],
      }),
    );

    render(<RoutinesPage />);
    expect(screen.getByText("Procedure")).toBeInTheDocument();
    expect(screen.queryByText("Unknown")).not.toBeInTheDocument();
  });

  it("shows the routines count in the heading", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ routinesCount: 4, allRoutines: [] }));

    render(<RoutinesPage />);
    expect(screen.getByText("(4)")).toBeInTheDocument();
    expect(screen.getByText("No routines.")).toBeInTheDocument();
  });
});
