import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { RelationshipsPage } from "@/routes/relationships";
import { failToLoad, renderWithClient } from "@/test/utils";
import type { GraphTable, RelationshipGraph, RelationshipsSummary } from "@/types/report";

// ELK layout does not run under a headless DOM; report the props the diagram was handed instead.
vi.mock("@/components/RelationshipDiagram", () => ({
  RelationshipDiagram: ({ graph, compact }: { graph: RelationshipGraph; compact: boolean }) => (
    <div data-testid="diagram" data-compact={String(compact)}>
      {graph.nodes.map((n) => n.id).join(",")}
    </div>
  ),
}));

function node(id: string): GraphTable {
  return {
    id,
    name: id,
    tableUrl: `#/tables/${id}`,
    columns: [],
    columnsCount: 0,
    parentKeysCount: 0,
    childKeysCount: 0,
  };
}

function graphOf(...ids: string[]): RelationshipsSummary {
  const nodes = ids.map(node);
  return { graph: { nodes, nodesCount: nodes.length, edges: [], edgesCount: 0 } };
}

function renderRelationshipsPage(summary?: RelationshipsSummary) {
  return renderWithClient(<RelationshipsPage />, {
    data: { summaries: summary === undefined ? {} : { relationships: summary } },
  });
}

describe("RelationshipsPage", () => {
  it("shows a loading indicator while pending", () => {
    renderRelationshipsPage();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    renderRelationshipsPage();
    expect(await screen.findByText("Failed to load relationships: boom")).toBeInTheDocument();
  });

  it("says so, and offers no view toggle, when the graph holds no tables", () => {
    renderRelationshipsPage(graphOf());
    expect(screen.getByText("No relationship diagrams available.")).toBeInTheDocument();
    expect(screen.queryByTestId("diagram")).not.toBeInTheDocument();
    expect(screen.queryByRole("button", { name: "Compact" })).not.toBeInTheDocument();
  });

  it("draws the graph it was given", () => {
    renderRelationshipsPage(graphOf("actor", "film"));
    expect(screen.getByTestId("diagram")).toHaveTextContent("actor,film");
  });

  it("starts in the compact view", () => {
    renderRelationshipsPage(graphOf("actor"));
    expect(screen.getByTestId("diagram")).toHaveAttribute("data-compact", "true");
  });

  it("switches to the large view and back", async () => {
    const user = userEvent.setup();
    renderRelationshipsPage(graphOf("actor"));

    await user.click(screen.getByRole("button", { name: "Large" }));
    expect(screen.getByTestId("diagram")).toHaveAttribute("data-compact", "false");

    await user.click(screen.getByRole("button", { name: "Compact" }));
    expect(screen.getByTestId("diagram")).toHaveAttribute("data-compact", "true");
  });
});
