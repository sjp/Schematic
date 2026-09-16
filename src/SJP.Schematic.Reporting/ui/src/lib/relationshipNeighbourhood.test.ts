import { describe, expect, it } from "vitest";

import {
  getTableDegrees,
  getTableDiagrams,
  selectNeighbourhood,
} from "@/lib/relationshipNeighbourhood";
import type { GraphEdge, GraphTable, RelationshipGraph } from "@/types/report";

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

function edge(childTableId: string, parentTableId: string): GraphEdge {
  return {
    id: `${childTableId}->${parentTableId}`,
    childTableId,
    parentTableId,
    constraintName: "",
    childColumns: [],
    parentColumns: [],
  };
}

function graphOf(nodeIds: string[], edges: GraphEdge[]): RelationshipGraph {
  const nodes = nodeIds.map(node);
  return { nodes, nodesCount: nodes.length, edges, edgesCount: edges.length };
}

// actor <- film_actor -> film <- inventory
const sakilaChain = graphOf(
  ["actor", "film_actor", "film", "inventory"],
  [edge("film_actor", "actor"), edge("film_actor", "film"), edge("inventory", "film")],
);

describe("getTableDegrees", () => {
  it("returns only the focal table for zero degrees", () => {
    expect([...getTableDegrees(sakilaChain, "actor", 0)]).toEqual([["actor", 0]]);
  });

  it("follows references from a child table to its parent", () => {
    expect([...getTableDegrees(sakilaChain, "film_actor", 1)]).toEqual([
      ["film_actor", 0],
      ["actor", 1],
      ["film", 1],
    ]);
  });

  it("follows references from a parent table to its children", () => {
    expect([...getTableDegrees(sakilaChain, "actor", 1)]).toEqual([
      ["actor", 0],
      ["film_actor", 1],
    ]);
  });

  it("widens the set by one hop per degree", () => {
    expect([...getTableDegrees(sakilaChain, "actor", 2)]).toEqual([
      ["actor", 0],
      ["film_actor", 1],
      ["film", 2],
    ]);
  });

  it("lists children before parents at the same degree", () => {
    expect([...getTableDegrees(sakilaChain, "film", 1)]).toEqual([
      ["film", 0],
      ["film_actor", 1],
      ["inventory", 1],
    ]);
  });

  it("keeps the nearest degree for a table reachable by several paths", () => {
    const triangle = graphOf(["a", "b", "c"], [edge("b", "a"), edge("c", "b"), edge("c", "a")]);

    expect(getTableDegrees(triangle, "a", 2).get("c")).toBe(1);
  });

  it("handles a table that references itself", () => {
    const selfReferencing = graphOf(["staff"], [edge("staff", "staff")]);

    expect([...getTableDegrees(selfReferencing, "staff", 2)]).toEqual([["staff", 0]]);
  });

  it("returns nothing for a table that is not in the graph", () => {
    expect(getTableDegrees(sakilaChain, "missing", 2).size).toBe(0);
  });
});

describe("selectNeighbourhood", () => {
  it("highlights only the focal table", () => {
    const degrees = getTableDegrees(sakilaChain, "actor", 1);

    const neighbourhood = selectNeighbourhood(sakilaChain, degrees, 1);

    expect(neighbourhood.nodes.map((n) => [n.id, n.isHighlighted ?? false])).toEqual([
      ["actor", true],
      ["film_actor", false],
    ]);
  });

  it("does not change the nodes of the source graph", () => {
    const degrees = getTableDegrees(sakilaChain, "actor", 1);

    selectNeighbourhood(sakilaChain, degrees, 1);

    expect(sakilaChain.nodes.some((n) => n.isHighlighted === true)).toBe(false);
  });

  it("keeps only edges whose tables are both included", () => {
    const degrees = getTableDegrees(sakilaChain, "actor", 1);

    const neighbourhood = selectNeighbourhood(sakilaChain, degrees, 1);

    expect(neighbourhood.edges.map((e) => e.id)).toEqual(["film_actor->actor"]);
    expect(neighbourhood.nodesCount).toBe(2);
    expect(neighbourhood.edgesCount).toBe(1);
  });
});

describe("getTableDiagrams", () => {
  it("returns a one-degree and a two-degree diagram", () => {
    const diagrams = getTableDiagrams(sakilaChain, "actor");

    expect(diagrams.map((d) => [d.name, d.graph.nodes.map((n) => n.id)])).toEqual([
      ["One Degree", ["actor", "film_actor"]],
      ["Two Degrees", ["actor", "film_actor", "film"]],
    ]);
  });

  it("includes every sibling of a hub table two degrees away", () => {
    const childIds = Array.from({ length: 50 }, (_, i) => `child_${i}`);
    const hubGraph = graphOf(
      ["hub", ...childIds],
      childIds.map((id) => edge(id, "hub")),
    );

    const [oneDegree, twoDegrees] = getTableDiagrams(hubGraph, "child_0");

    expect(oneDegree?.graph.nodes.map((n) => n.id)).toEqual(["child_0", "hub"]);
    expect(twoDegrees?.graph.nodesCount).toBe(51);
    expect(twoDegrees?.graph.edgesCount).toBe(50);
  });

  it("returns empty diagrams for a table that is not in the graph", () => {
    const diagrams = getTableDiagrams(sakilaChain, "missing");

    expect(diagrams.map((d) => d.graph.nodesCount)).toEqual([0, 0]);
  });
});
