import { useNavigate } from "@tanstack/react-router";
import {
  Background,
  Controls,
  Handle,
  MarkerType,
  Position,
  ReactFlow,
  type Edge,
  type Node,
  type NodeProps,
} from "@xyflow/react";

import "@xyflow/react/dist/style.css";
import { KeyRound, Link2, ShieldCheck } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import { Button } from "@/components/ui/button";
import { layoutElkGraph } from "@/lib/elkLayout";
import { cn } from "@/lib/utils";
import type { GraphColumn, GraphTable, RelationshipGraph } from "@/types/report";

/**
 * Diagrams with more tables than this are only laid out once the reader asks for them, and use a
 * faster node placement. Laying out 1000 tables with network simplex placement takes about 20 times
 * as long as with Brandes-Köpf placement, which trades that for a taller drawing.
 */
export const LARGE_DIAGRAM_TABLE_COUNT = 300;

// Node geometry. ELK needs concrete node sizes up front, and the custom node renders at exactly
// these dimensions so the routed edges line up with the boxes.
const HEADER_HEIGHT = 34;
const ROW_HEIGHT = 22;
const FOOTER_HEIGHT = 26;
const CHAR_WIDTH = 7;
const MIN_WIDTH = 180;
const MAX_WIDTH = 380;

type TableNodeData = {
  table: GraphTable;
  columns: GraphColumn[];
  width: number;
};

type TableFlowNode = Node<TableNodeData, "table">;

function nodeSize(table: GraphTable, columns: GraphColumn[]) {
  const longestContent = columns.reduce(
    (max, c) => Math.max(max, c.name.length + c.type.length + 4),
    table.name.length + 6,
  );
  const width = Math.min(MAX_WIDTH, Math.max(MIN_WIDTH, longestContent * CHAR_WIDTH + 28));
  const height = HEADER_HEIGHT + columns.length * ROW_HEIGHT + FOOTER_HEIGHT;
  return { width, height };
}

/** A single table rendered as a React Flow node: header, column rows with key badges, and a footer. */
function TableNodeComponent({ data }: NodeProps<TableFlowNode>) {
  const { table, columns, width } = data;
  return (
    <div
      style={{ width }}
      className={cn(
        "overflow-hidden rounded-md border bg-card text-xs shadow-sm",
        table.isHighlighted ? "border-primary ring-2 ring-primary/40" : "border-border",
      )}
    >
      {/* Both handles exist on every node so any table can be either endpoint of an edge. */}
      <Handle type="target" position={Position.Left} className="!border-0 !bg-muted-foreground" />
      <Handle type="source" position={Position.Right} className="!border-0 !bg-muted-foreground" />
      <div
        className={cn(
          "truncate border-b px-2 py-1.5 font-semibold",
          table.isHighlighted ? "border-primary bg-primary text-primary-foreground" : "bg-muted",
        )}
        title={table.name}
      >
        {table.name}
      </div>
      <div>
        {columns.length === 0 ? (
          <div className="px-2 py-1 text-muted-foreground italic">no key columns</div>
        ) : (
          columns.map((c) => (
            <div
              key={c.name}
              className="flex items-center gap-1 border-b border-border/40 px-2 py-0.5 last:border-b-0"
            >
              {c.isPrimaryKey && (
                <KeyRound className="size-3 shrink-0 text-amber-500" aria-label="Primary key" />
              )}
              {c.isUniqueKey && (
                <ShieldCheck className="size-3 shrink-0 text-sky-500" aria-label="Unique key" />
              )}
              {c.isForeignKey && (
                <Link2 className="size-3 shrink-0 text-emerald-500" aria-label="Foreign key" />
              )}
              <span className={cn("truncate", (c.isPrimaryKey || c.isUniqueKey) && "font-medium")}>
                {c.name}
              </span>
              <span className="ml-auto truncate pl-2 text-muted-foreground">{c.type}</span>
            </div>
          ))
        )}
      </div>
      <div className="flex justify-between border-t bg-muted/50 px-2 py-1 text-muted-foreground">
        <span title="parent keys · child keys">
          {table.parentKeysCount} ▴ {table.childKeysCount} ▾
        </span>
      </div>
    </div>
  );
}

const nodeTypes = { table: TableNodeComponent };

type LayoutResult = { nodes: TableFlowNode[]; edges: Edge[] };

async function layoutGraph(
  graph: RelationshipGraph,
  compact: boolean,
  signal: AbortSignal,
): Promise<LayoutResult> {
  const prepared = graph.nodes.map((table) => {
    const columns = compact ? table.columns.filter((c) => c.isKey) : table.columns;
    const { width, height } = nodeSize(table, columns);
    return { table, columns, width, height };
  });

  const sizeById = new Map(prepared.map((p) => [p.table.id, p]));

  const elkGraph = {
    id: "root",
    layoutOptions: {
      "elk.algorithm": "layered",
      // Left-to-right: child (referencing) tables flow toward their parents on the right.
      "elk.direction": "RIGHT",
      "elk.layered.spacing.nodeNodeBetweenLayers": "90",
      "elk.spacing.nodeNode": "45",
      "elk.layered.nodePlacement.strategy":
        graph.nodes.length > LARGE_DIAGRAM_TABLE_COUNT ? "BRANDES_KOEPF" : "NETWORK_SIMPLEX",
    },
    children: prepared.map((p) => ({
      id: p.table.id,
      width: p.width,
      height: p.height,
    })),
    edges: graph.edges.map((e) => ({
      id: e.id,
      sources: [e.childTableId],
      targets: [e.parentTableId],
    })),
  };

  const laidOut = await layoutElkGraph(elkGraph, signal);

  const nodes: TableFlowNode[] = (laidOut.children ?? []).map((child) => {
    const p = sizeById.get(child.id)!;
    return {
      id: child.id,
      type: "table",
      position: { x: child.x ?? 0, y: child.y ?? 0 },
      width: p.width,
      height: child.height ?? p.height,
      data: { table: p.table, columns: p.columns, width: p.width },
    };
  });

  const edges: Edge[] = graph.edges.map((e) => ({
    id: e.id,
    source: e.childTableId,
    target: e.parentTableId,
    markerEnd: { type: MarkerType.ArrowClosed, width: 16, height: 16 },
    style: { stroke: "var(--muted-foreground)" },
  }));

  return { nodes, edges };
}

// Finished layouts, so switching back to a graph or column set already shown does not lay it out
// again. Entries go away with the graph they were computed for.
const layoutCache = new WeakMap<RelationshipGraph, Map<boolean, LayoutResult>>();

function getCachedLayout(graph: RelationshipGraph, compact: boolean) {
  return layoutCache.get(graph)?.get(compact);
}

function setCachedLayout(graph: RelationshipGraph, compact: boolean, layout: LayoutResult) {
  let layouts = layoutCache.get(graph);
  if (layouts === undefined) {
    layouts = new Map();
    layoutCache.set(graph, layouts);
  }
  layouts.set(compact, layout);
}

type LayoutState = (LayoutResult | { error: unknown }) & {
  forGraph: RelationshipGraph;
  forCompact: boolean;
};

/**
 * Renders a relationship diagram from graph data. ELK computes a layered layout (replacing the
 * Graphviz `dot` layout) off the main thread and React Flow draws it with interactive pan/zoom;
 * clicking a table node navigates to its detail page. Theming follows the app's light/dark CSS
 * variables, so no SVG recolouring is needed. A diagram with more than
 * {@link LARGE_DIAGRAM_TABLE_COUNT} tables waits for the reader to ask for it.
 */
export function RelationshipDiagram({
  graph,
  compact = false,
}: {
  graph: RelationshipGraph;
  compact?: boolean;
}) {
  const navigate = useNavigate();
  const [layout, setLayout] = useState<LayoutState | null>(null);
  const [requestedGraph, setRequestedGraph] = useState<RelationshipGraph | null>(null);

  const isEmpty = useMemo(() => graph.nodes.length === 0, [graph]);
  const cached = getCachedLayout(graph, compact);
  const isLarge = graph.nodes.length > LARGE_DIAGRAM_TABLE_COUNT;
  const awaitingRequest = isLarge && requestedGraph !== graph && cached === undefined;

  // Re-layout whenever the graph or the compact toggle changes, unless there is nothing to draw, the
  // layout is cached, or a large diagram has not been asked for. The result is tagged with the
  // inputs it was computed for so a stale layout is never shown, and aborting drops (and where
  // possible stops) a superseded layout. State is only set from the async callback (never
  // synchronously in the effect).
  useEffect(() => {
    if (isEmpty || awaitingRequest || cached !== undefined) return;

    const controller = new AbortController();
    layoutGraph(graph, compact, controller.signal).then(
      (result) => {
        setCachedLayout(graph, compact, result);
        if (!controller.signal.aborted) {
          setLayout({ ...result, forGraph: graph, forCompact: compact });
        }
      },
      (error: unknown) => {
        if (!controller.signal.aborted) {
          setLayout({ error, forGraph: graph, forCompact: compact });
        }
      },
    );
    return () => controller.abort();
  }, [graph, compact, isEmpty, awaitingRequest, cached]);

  const current =
    cached ??
    (layout !== null && layout.forGraph === graph && layout.forCompact === compact
      ? layout
      : undefined);

  if (isEmpty) {
    return (
      <div className="rounded-md border bg-card p-6 text-sm text-muted-foreground">
        No related tables to diagram.
      </div>
    );
  }

  return (
    <div className="h-[600px] overflow-hidden rounded-md border bg-card">
      {awaitingRequest ? (
        <div className="flex h-full flex-col items-center justify-center gap-3 p-6 text-center text-sm text-muted-foreground">
          <p>
            This diagram has {graph.nodes.length} tables. Laying it out can take a while, and the
            page may respond slowly while it is shown.
          </p>
          <Button size="sm" onClick={() => setRequestedGraph(graph)}>
            Show diagram
          </Button>
        </div>
      ) : current === undefined ? (
        <div className="flex h-full items-center justify-center text-sm text-muted-foreground">
          Laying out diagram…
        </div>
      ) : "error" in current ? (
        <div className="flex h-full items-center justify-center p-6 text-sm text-destructive">
          The diagram could not be laid out:{" "}
          {current.error instanceof Error ? current.error.message : String(current.error)}
        </div>
      ) : (
        <ReactFlow
          nodes={current.nodes}
          edges={current.edges}
          nodeTypes={nodeTypes}
          onNodeClick={(_, node) => {
            void navigate({
              to: "/tables/$tableKey",
              params: { tableKey: node.id },
            });
          }}
          nodesConnectable={false}
          fitView
          minZoom={0.1}
          proOptions={{ hideAttribution: true }}
        >
          <Background />
          <Controls showInteractive={false} />
        </ReactFlow>
      )}
    </div>
  );
}
