import { useState } from "react";
import { Share2 } from "lucide-react";
import { RelationshipDiagram } from "@/components/RelationshipDiagram";
import { Button } from "@/components/ui/button";
import { useSummary } from "@/hooks/useReportData";
import type { RelationshipsSummary } from "@/types/report";

export function RelationshipsPage() {
  const { data, isPending, isError, error } =
    useSummary<RelationshipsSummary>("relationships");

  // "Compact" shows key columns only; "Large" shows every column. Both are views over the same
  // graph payload, toggled client-side.
  const [compact, setCompact] = useState(true);

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return (
      <p className="text-destructive">
        Failed to load relationships: {error.message}
      </p>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Share2 className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Relationships</h1>
      </div>

      {data.graph.nodes.length === 0 ? (
        <p className="text-muted-foreground">
          No relationship diagrams available.
        </p>
      ) : (
        <>
          <div className="flex gap-2">
            <Button
              variant={compact ? "default" : "outline"}
              size="sm"
              onClick={() => setCompact(true)}
            >
              Compact
            </Button>
            <Button
              variant={compact ? "outline" : "default"}
              size="sm"
              onClick={() => setCompact(false)}
            >
              Large
            </Button>
          </div>
          <RelationshipDiagram graph={data.graph} compact={compact} />
        </>
      )}
    </div>
  );
}
