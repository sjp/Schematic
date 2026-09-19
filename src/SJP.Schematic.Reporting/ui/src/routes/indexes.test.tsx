import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { IndexesPage } from "@/routes/indexes";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { IndexRow, IndexesSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

const mockUseSummary = vi.mocked(useSummary<IndexesSummary>);

const INDEX: IndexRow = {
  name: "idx_actor_last_name",
  tableName: "actor",
  tableUrl: "#/tables/actor-d4592e62",
  isUnique: false,
  columnsText: "last_name",
  includedColumnsText: "first_name",
  indexType: "B-Tree",
  filterText: "last_name IS NOT NULL",
  isEnabled: true,
  isValid: true,
  isVisible: true,
};

describe("IndexesPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<IndexesPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("boom")));

    render(<IndexesPage />);
    expect(screen.getByText("Failed to load indexes: boom")).toBeInTheDocument();
  });

  it("links the owning table and shows the index's columns, type and filter", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ indexesCount: 1, tableIndexes: [INDEX] }));

    render(<IndexesPage />);
    expect(screen.getByRole("link", { name: "actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
    expect(screen.getByText("idx_actor_last_name")).toBeInTheDocument();
    expect(screen.getByText("last_name")).toBeInTheDocument();
    expect(screen.getByText("first_name")).toBeInTheDocument();
    expect(screen.getByText("B-Tree")).toBeInTheDocument();
    expect(screen.getByText("last_name IS NOT NULL")).toBeInTheDocument();
  });

  it("distinguishes unique indexes from non-unique ones", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        indexesCount: 2,
        tableIndexes: [INDEX, { ...INDEX, name: "actor_pkey", isUnique: true }],
      }),
    );

    render(<IndexesPage />);
    expect(screen.getByLabelText("Unique")).toBeInTheDocument();
    expect(screen.getByLabelText("Not unique")).toBeInTheDocument();
  });

  it("shows an em dash for an unreported index type and an unfiltered index", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        indexesCount: 1,
        tableIndexes: [{ ...INDEX, indexType: "", filterText: "" }],
      }),
    );

    render(<IndexesPage />);
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("carries an index's usability through to its status cell", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        indexesCount: 2,
        tableIndexes: [INDEX, { ...INDEX, name: "idx_stale", isValid: false }],
      }),
    );

    render(<IndexesPage />);
    expect(screen.getByText("Usable")).toBeInTheDocument();
    expect(screen.getByText("Invalid")).toBeInTheDocument();
  });

  it("shows the indexes count in the heading", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ indexesCount: 9, tableIndexes: [] }));

    render(<IndexesPage />);
    expect(screen.getByText("(9)")).toBeInTheDocument();
    expect(screen.getByText("No indexes.")).toBeInTheDocument();
  });
});
