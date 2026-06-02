import { useMemo, useState } from "react";
import { Share2 } from "lucide-react";
import { Diagram } from "@/components/Diagram";
import { Button } from "@/components/ui/button";
import { useSummary } from "@/hooks/useReportData";
import type { RelationshipsSummary } from "@/types/report";

export function RelationshipsPage() {
  const { data, isPending, isError, error } =
    useSummary<RelationshipsSummary>("relationships");

  // Default to the diagram flagged active by the renderer (Compact), else the first; an explicit
  // user selection takes precedence over the data-derived default.
  const [selectedLevel, setSelectedLevel] = useState<number | null>(null);
  const defaultLevel = useMemo(() => {
    const idx = data?.diagrams.findIndex((d) => d.isActive) ?? -1;
    return idx > 0 ? idx : 0;
  }, [data]);
  const activeLevel = selectedLevel ?? defaultLevel;

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

  const diagram = data.diagrams[activeLevel] ?? data.diagrams[0];

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Share2 className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Relationships</h1>
      </div>

      {data.diagrams.length === 0 || !diagram ? (
        <p className="text-muted-foreground">
          No relationship diagrams available.
        </p>
      ) : (
        <>
          {data.diagrams.length > 1 && (
            <div className="flex gap-2">
              {data.diagrams.map((d, i) => (
                <Button
                  key={d.containerId}
                  variant={i === activeLevel ? "default" : "outline"}
                  size="sm"
                  onClick={() => setSelectedLevel(i)}
                >
                  {d.name}
                </Button>
              ))}
            </div>
          )}
          <Diagram
            src={diagram.svgFile}
            title={`Relationships — ${diagram.name}`}
          />
        </>
      )}
    </div>
  );
}
