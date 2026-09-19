import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { OrphansPage } from "@/routes/orphans";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { OrphansSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

const mockUseSummary = vi.mocked(useSummary<OrphansSummary>);

describe("OrphansPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<OrphansPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("boom")));

    render(<OrphansPage />);
    expect(screen.getByText("Failed to load orphan tables: boom")).toBeInTheDocument();
  });

  it("links each orphan table to its own page and shows its column count", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        tablesCount: 1,
        tables: [{ name: "audit_log", tableUrl: "#/tables/audit_log-1a2b", columnCount: 5 }],
      }),
    );

    render(<OrphansPage />);
    expect(screen.getByRole("link", { name: "audit_log" })).toHaveAttribute(
      "href",
      "#/tables/audit_log-1a2b",
    );
    expect(screen.getByText("5")).toBeInTheDocument();
  });

  it("explains what makes a table an orphan", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ tablesCount: 0, tables: [] }));

    render(<OrphansPage />);
    expect(
      screen.getByText(
        "Tables that participate in no relationships (no foreign keys to or from them).",
      ),
    ).toBeInTheDocument();
  });

  it("shows the orphan count in the heading, and says so when there are none", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ tablesCount: 0, tables: [] }));

    render(<OrphansPage />);
    expect(screen.getByText("(0)")).toBeInTheDocument();
    expect(screen.getByText("No orphan tables.")).toBeInTheDocument();
  });
});
