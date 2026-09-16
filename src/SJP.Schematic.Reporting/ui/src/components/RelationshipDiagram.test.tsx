import { act, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ElkNode } from "elkjs/lib/elk-api.js";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { LARGE_DIAGRAM_TABLE_COUNT, RelationshipDiagram } from "@/components/RelationshipDiagram";
import { layoutElkGraph } from "@/lib/elkLayout";
import type { GraphTable, RelationshipGraph } from "@/types/report";

vi.mock("@/lib/elkLayout", () => ({ layoutElkGraph: vi.fn<typeof layoutElkGraph>() }));

vi.mock("@tanstack/react-router", () => ({
  useNavigate: () => vi.fn<(options: unknown) => void>(),
}));

// React Flow needs real layout measurements; the diagram's own logic only decides what it is given.
vi.mock("@xyflow/react", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@xyflow/react")>()),
  ReactFlow: ({ nodes }: { nodes: { id: string; data: { columns: unknown[] } }[] }) => (
    <ul data-testid="flow">
      {nodes.map((n) => (
        <li key={n.id}>
          {n.id}:{n.data.columns.length}
        </li>
      ))}
    </ul>
  ),
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
});
