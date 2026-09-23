import { act, render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ElkNode } from "elkjs/lib/elk-api.js";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { LARGE_DIAGRAM_TABLE_COUNT, RelationshipDiagram } from "@/components/RelationshipDiagram";
import { layoutElkGraph } from "@/lib/elkLayout";
import type { GraphColumn, GraphTable, RelationshipGraph } from "@/types/report";

/** The node data the diagram hands React Flow, as much of it as the mock below touches. */
type MockNode = { id: string; data: { table: GraphTable; columns: GraphColumn[] } };

vi.mock("@/lib/elkLayout", () => ({ layoutElkGraph: vi.fn<typeof layoutElkGraph>() }));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => vi.fn<(options: unknown) => void>(),
}));

// React Flow needs real layout measurements; the diagram's own logic only decides what it is given.
// The custom table node is still rendered, through the `nodeTypes` the diagram registers, so what a
// node draws is covered — but outside the list the layout assertions read, so they stay unaffected.
vi.mock("@xyflow/react", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@xyflow/react")>()),
  // A `Handle` reaches into a live React Flow store, which the mocked canvas does not set up.
  Handle: () => null,
  ReactFlow: ({
    nodes,
    nodeTypes,
  }: {
    nodes: MockNode[];
    nodeTypes: { table: (props: { data: MockNode["data"] }) => React.ReactNode };
  }) => {
    const TableNode = nodeTypes.table;
    return (
      <>
        <ul data-testid="flow">
          {nodes.map((n) => (
            <li key={n.id}>
              {n.id}:{n.data.columns.length}
            </li>
          ))}
        </ul>
        <div data-testid="table-nodes">
          {/* Only the first few: drawing every node of a large diagram costs ~0.15 s a test, and
              the node assertions only need one. */}
          {nodes.slice(0, 3).map((n) => (
            <TableNode key={n.id} data={n.data} />
          ))}
        </div>
      </>
    );
  },
  Background: () => null,
  Controls: () => null,
}));

const noop = () => {};

const mockLayout = vi.mocked(layoutElkGraph);

function makeGraph(tableCount: number): RelationshipGraph {
  const nodes: GraphTable[] = Array.from({ length: tableCount }, (_, i) => ({
    id: `t${i}`,
    name: `table_${i}`,
    tableUrl: `#/tables/t${i}`,
    columns: [
      {
        name: "id",
        type: "int",
        isNullable: false,
        isPrimaryKey: true,
        isUniqueKey: false,
        isForeignKey: false,
        isKey: true,
      },
      {
        name: "name",
        type: "text",
        isNullable: true,
        isPrimaryKey: false,
        isUniqueKey: false,
        isForeignKey: false,
        isKey: false,
      },
    ],
    columnsCount: 2,
    parentKeysCount: 0,
    childKeysCount: 0,
  }));
  return { nodes, nodesCount: tableCount, edges: [], edgesCount: 0 };
}

/** A key column, which the compact view keeps and the node draws a badge for. */
const column = (name: string, type: string, flags: Partial<GraphColumn> = {}): GraphColumn => ({
  name,
  type,
  isNullable: false,
  isPrimaryKey: false,
  isUniqueKey: false,
  isForeignKey: false,
  isKey: true,
  ...flags,
});

/** One table carrying a column of every kind the node draws a badge for. */
function oneTableGraph(): RelationshipGraph {
  return {
    nodes: [
      {
        id: "film_actor",
        name: "film_actor",
        tableUrl: "#/tables/film_actor",
        columns: [
          column("actor_id", "int", { isPrimaryKey: true }),
          column("film_id", "int", { isUniqueKey: true }),
          column("language_id", "int", { isForeignKey: true }),
          column("last_update", "timestamp"),
        ],
        columnsCount: 4,
        parentKeysCount: 2,
        childKeysCount: 3,
      },
    ],
    nodesCount: 1,
    edges: [],
    edgesCount: 0,
  };
}

/** Resolves every layout with the graph as given, positioned at the origin. */
function layOutImmediately() {
  mockLayout.mockImplementation((graph: ElkNode) =>
    Promise.resolve({
      ...graph,
      children: graph.children?.map((c) => ({ ...c, x: 0, y: 0 })),
    }),
  );
}

function placementOf(call: number) {
  return mockLayout.mock.calls[call]![0].layoutOptions!["elk.layered.nodePlacement.strategy"];
}

describe("RelationshipDiagram", () => {
  beforeEach(() => {
    mockLayout.mockReset();
  });

  it("lays out a diagram with network simplex placement and draws it", async () => {
    layOutImmediately();

    render(<RelationshipDiagram graph={makeGraph(2)} />);

    expect(await screen.findByTestId("flow")).toHaveTextContent("t0:2t1:2");
    expect(placementOf(0)).toBe("NETWORK_SIMPLEX");
  });

  it("waits to be asked before laying out a large diagram, then uses a faster placement", async () => {
    layOutImmediately();
    const user = userEvent.setup();

    render(<RelationshipDiagram graph={makeGraph(LARGE_DIAGRAM_TABLE_COUNT + 1)} compact />);

    expect(screen.getByText(/This diagram has 301 tables/u)).toBeInTheDocument();
    expect(mockLayout).not.toHaveBeenCalled();

    await user.click(screen.getByRole("button", { name: "Show diagram" }));

    expect(await screen.findByTestId("flow")).toBeInTheDocument();
    expect(placementOf(0)).toBe("BRANDES_KOEPF");
  });

  it("does not ask again when switching columns on a large diagram already shown", async () => {
    layOutImmediately();
    const user = userEvent.setup();
    const graph = makeGraph(LARGE_DIAGRAM_TABLE_COUNT + 1);

    const { rerender } = render(<RelationshipDiagram graph={graph} compact />);
    await user.click(screen.getByRole("button", { name: "Show diagram" }));
    await screen.findByTestId("flow");

    rerender(<RelationshipDiagram graph={graph} compact={false} />);

    expect(screen.queryByRole("button", { name: "Show diagram" })).not.toBeInTheDocument();
    expect(await screen.findByTestId("flow")).toHaveTextContent("t0:2");
    expect(mockLayout).toHaveBeenCalledTimes(2);
  });

  it("reuses a finished layout when switching back to it", async () => {
    layOutImmediately();
    const graph = makeGraph(3);

    const { rerender } = render(<RelationshipDiagram graph={graph} compact />);
    expect(await screen.findByTestId("flow")).toHaveTextContent("t0:1");

    rerender(<RelationshipDiagram graph={graph} compact={false} />);
    expect(await screen.findByTestId("flow")).toHaveTextContent("t0:2");

    rerender(<RelationshipDiagram graph={graph} compact />);
    expect(screen.getByTestId("flow")).toHaveTextContent("t0:1");
    expect(mockLayout).toHaveBeenCalledTimes(2);
  });

  it("shows a cached large diagram without asking", async () => {
    layOutImmediately();
    const user = userEvent.setup();
    const graph = makeGraph(LARGE_DIAGRAM_TABLE_COUNT + 1);

    const first = render(<RelationshipDiagram graph={graph} compact />);
    await user.click(screen.getByRole("button", { name: "Show diagram" }));
    await screen.findByTestId("flow");
    first.unmount();

    render(<RelationshipDiagram graph={graph} compact />);

    expect(screen.getByTestId("flow")).toBeInTheDocument();
    expect(mockLayout).toHaveBeenCalledTimes(1);
  });

  it("abandons a layout that is superseded before it finishes", () => {
    const signals: AbortSignal[] = [];
    mockLayout.mockImplementation((_graph, signal) => {
      signals.push(signal!);
      return new Promise(() => {});
    });

    const { rerender } = render(<RelationshipDiagram graph={makeGraph(2)} />);
    rerender(<RelationshipDiagram graph={makeGraph(2)} />);

    expect(screen.getByText("Laying out diagram…")).toBeInTheDocument();
    expect(signals).toHaveLength(2);
    expect(signals[0]!.aborted).toBe(true);
    expect(signals[1]!.aborted).toBe(false);
  });

  it("reports a layout that fails", async () => {
    let fail: (error: Error) => void = noop;
    mockLayout.mockImplementation(
      () =>
        new Promise((_, reject) => {
          fail = reject;
        }),
    );

    render(<RelationshipDiagram graph={makeGraph(2)} />);
    // `act` only flushes pending promises when its callback is thenable, and `async` is what
    // makes this one thenable.
    // oxlint-disable-next-line typescript/require-await
    await act(async () => {
      fail(new Error("worker crashed"));
    });

    expect(screen.getByText(/could not be laid out: worker crashed/u)).toBeInTheDocument();
  });

  it("says so when there are no tables", () => {
    render(<RelationshipDiagram graph={makeGraph(0)} />);

    expect(screen.getByText("No related tables to diagram.")).toBeInTheDocument();
    expect(mockLayout).not.toHaveBeenCalled();
  });

  it("draws a table node with its name, key badges, column types and key counts", async () => {
    layOutImmediately();

    render(<RelationshipDiagram graph={oneTableGraph()} />);
    await screen.findByTestId("flow");

    const node = screen.getByTestId("table-nodes");
    expect(within(node).getByText("film_actor")).toBeInTheDocument();
    expect(within(node).getByLabelText("Primary key")).toBeInTheDocument();
    expect(within(node).getByLabelText("Unique key")).toBeInTheDocument();
    expect(within(node).getByLabelText("Foreign key")).toBeInTheDocument();
    expect(within(node).getByText("last_update")).toBeInTheDocument();
    expect(within(node).getByText("timestamp")).toBeInTheDocument();
    expect(within(node).getByText("2 ▴ 3 ▾")).toBeInTheDocument();
  });

  it("highlights the focal table of a per-table diagram", async () => {
    layOutImmediately();
    const graph = oneTableGraph();

    const { rerender } = render(<RelationshipDiagram graph={graph} />);
    await screen.findByTestId("flow");
    expect(screen.getByTitle("film_actor")).not.toHaveClass("bg-primary");

    const highlighted = oneTableGraph();
    highlighted.nodes[0]!.isHighlighted = true;
    rerender(<RelationshipDiagram graph={highlighted} />);
    await screen.findByTestId("flow");

    expect(screen.getByTitle("film_actor")).toHaveClass("bg-primary");
  });

  it("says so for a table the compact view leaves no columns of", async () => {
    layOutImmediately();
    const graph = oneTableGraph();
    for (const c of graph.nodes[0]!.columns) {
      c.isKey = false;
    }

    render(<RelationshipDiagram graph={graph} compact />);
    await screen.findByTestId("flow");

    expect(screen.getByText("no key columns")).toBeInTheDocument();
  });
});
