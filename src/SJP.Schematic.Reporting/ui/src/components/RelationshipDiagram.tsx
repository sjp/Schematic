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
import ELK from "elkjs/lib/elk.bundled.js";
import { KeyRound, Link2, ShieldCheck } from "lucide-react";
import { useEffect, useMemo, useState } from "react";

import { cn } from "@/lib/utils";
import type { GraphColumn, GraphTable, RelationshipGraph } from "@/types/report";

const elk = new ELK();

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

async function layoutGraph(graph: RelationshipGraph, compact: boolean): Promise<LayoutResult> {
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
      "elk.layered.nodePlacement.strategy": "NETWORK_SIMPLEX",
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

  const laidOut = await elk.layout(elkGraph);

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

type LayoutState = LayoutResult & {
  forGraph: RelationshipGraph;
  forCompact: boolean;
};

/**
 * Renders a relationship diagram from graph data. ELK computes a layered layout (replacing the
 * Graphviz `dot` layout) and React Flow draws it with interactive pan/zoom; clicking a table node
 * navigates to its detail page. Theming follows the app's light/dark CSS variables, so no SVG
 * recolouring is needed.
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

  // Re-layout whenever the graph or the compact toggle changes. The result is tagged with the
  // inputs it was computed for so a stale layout is never shown, and `cancelled` drops a superseded
  // async result. State is only set from the async callback (never synchronously in the effect).
  useEffect(() => {
    let cancelled = false;
    void layoutGraph(graph, compact).then((result) => {
      if (!cancelled) setLayout({ ...result, forGraph: graph, forCompact: compact });
    });
    return () => {
      cancelled = true;
    };
  }, [graph, compact]);

  const isEmpty = useMemo(() => graph.nodes.length === 0, [graph]);
  const ready = layout !== null && layout.forGraph === graph && layout.forCompact === compact;

  if (isEmpty) {
    return (
      <div className="rounded-md border bg-card p-6 text-sm text-muted-foreground">
        No related tables to diagram.
      </div>
    );
  }

  return (
    <div className="h-[600px] overflow-hidden rounded-md border bg-card">
      {!ready ? (
        <div className="flex h-full items-center justify-center text-sm text-muted-foreground">
          Laying out diagram…
        </div>
      ) : (
        <ReactFlow
          nodes={layout.nodes}
          edges={layout.edges}
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
