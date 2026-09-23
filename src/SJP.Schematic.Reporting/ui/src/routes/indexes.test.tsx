import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { IndexesPage } from "@/routes/indexes";
import { failToLoad, renderWithClient } from "@/test/utils";
import type { IndexRow, IndexesSummary } from "@/types/report";

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

function renderIndexesPage(summary?: IndexesSummary) {
  return renderWithClient(<IndexesPage />, {
    data: { summaries: summary === undefined ? {} : { indexes: summary } },
  });
}

describe("IndexesPage", () => {
  it("shows a loading indicator while pending", () => {
    renderIndexesPage();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    renderIndexesPage();
    expect(await screen.findByText("Failed to load indexes: boom")).toBeInTheDocument();
  });

  it("links the owning table and shows the index's columns, type and filter", () => {
    renderIndexesPage({ indexesCount: 1, tableIndexes: [INDEX] });
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
    renderIndexesPage({
      indexesCount: 2,
      tableIndexes: [INDEX, { ...INDEX, name: "actor_pkey", isUnique: true }],
    });
    expect(screen.getByLabelText("Unique")).toBeInTheDocument();
    expect(screen.getByLabelText("Not unique")).toBeInTheDocument();
  });

  it("shows an em dash for an unreported index type and an unfiltered index", () => {
    renderIndexesPage({
      indexesCount: 1,
      tableIndexes: [{ ...INDEX, indexType: "", filterText: "" }],
    });
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("carries an index's usability through to its status cell", () => {
    renderIndexesPage({
      indexesCount: 2,
      tableIndexes: [INDEX, { ...INDEX, name: "idx_stale", isValid: false }],
    });
    expect(screen.getByText("Usable")).toBeInTheDocument();
    expect(screen.getByText("Invalid")).toBeInTheDocument();
  });

  it("shows the indexes count in the heading", () => {
    renderIndexesPage({ indexesCount: 9, tableIndexes: [] });
    expect(screen.getByText("(9)")).toBeInTheDocument();
    expect(screen.getByText("No indexes.")).toBeInTheDocument();
  });
});
