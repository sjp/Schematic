import type { GraphTable, RelationshipGraph } from "@/types/report";

/** A per-table relationship diagram, e.g. "One Degree", drawn from part of the schema-wide graph. */
export interface NeighbourhoodDiagram {
  name: string;
  graph: RelationshipGraph;
}

/**
 * How many foreign-key hops each table is from `focalId`, following references in both directions
 * and stopping after `maxDegrees` hops. Tables are listed nearest first, in the order they were
 * reached. Empty when `focalId` is not a node of `graph`.
 */
export function getTableDegrees(
  graph: RelationshipGraph,
  focalId: string,
  maxDegrees: number,
): Map<string, number> {
  const degrees = new Map<string, number>();
  if (!graph.nodes.some((node) => node.id === focalId)) {
    return degrees;
  }

  // Children (referencing tables) are visited before parents, then everything in edge order, so a
  // diagram lists its tables in a stable order.
  const children = new Map<string, string[]>();
  const parents = new Map<string, string[]>();
  for (const edge of graph.edges) {
    appendTo(children, edge.parentTableId, edge.childTableId);
    appendTo(parents, edge.childTableId, edge.parentTableId);
  }

  degrees.set(focalId, 0);
  let frontier = [focalId];
  for (let degree = 1; degree <= maxDegrees && frontier.length > 0; degree++) {
    const next: string[] = [];
    for (const tableId of frontier) {
      for (const relatedId of [...(children.get(tableId) ?? []), ...(parents.get(tableId) ?? [])]) {
        if (!degrees.has(relatedId)) {
          degrees.set(relatedId, degree);
          next.push(relatedId);
        }
      }
    }
    frontier = next;
  }

  return degrees;
}

/**
 * The part of `graph` within `maxDegree` hops of the focal table, as measured by `degrees` (see
 * `getTableDegrees`). The focal table is the node at degree 0 and is marked as highlighted. An edge
 * is kept only when both of its tables are in the result.
 */
export function selectNeighbourhood(
  graph: RelationshipGraph,
  degrees: ReadonlyMap<string, number>,
  maxDegree: number,
): RelationshipGraph {
  const nodesById = new Map(graph.nodes.map((node) => [node.id, node]));

  const nodes: GraphTable[] = [];
  for (const [tableId, degree] of degrees) {
    const node = nodesById.get(tableId);
    if (node && degree <= maxDegree) {
      nodes.push(degree === 0 ? { ...node, isHighlighted: true } : node);
    }
  }

  const included = new Set(nodes.map((node) => node.id));
  const edges = graph.edges.filter(
    (edge) => included.has(edge.childTableId) && included.has(edge.parentTableId),
  );

  return { nodes, nodesCount: nodes.length, edges, edgesCount: edges.length };
}

/** The "One Degree" and "Two Degrees" diagrams shown on a table's detail page. */
export function getTableDiagrams(
  graph: RelationshipGraph,
  tableId: string,
): NeighbourhoodDiagram[] {
  const degrees = getTableDegrees(graph, tableId, 2);
  return [
    { name: "One Degree", graph: selectNeighbourhood(graph, degrees, 1) },
    { name: "Two Degrees", graph: selectNeighbourhood(graph, degrees, 2) },
  ];
}

function appendTo(lookup: Map<string, string[]>, key: string, value: string) {
  const values = lookup.get(key);
  if (values) {
    values.push(value);
  } else {
    lookup.set(key, [value]);
  }
}
