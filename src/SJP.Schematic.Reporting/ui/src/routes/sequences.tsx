import { useMemo } from "react";
import { Link } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, ListOrdered, Minus } from "lucide-react";
import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { SequenceSummary, SequencesSummary } from "@/types/report";

/** Extracts the route safeKey from a `#/sequences/<key>` hash url. */
function keyFromUrl(sequenceUrl: string): string {
  return sequenceUrl.slice(sequenceUrl.lastIndexOf("/") + 1);
}

export function SequencesPage() {
  const { data, isPending, isError, error } =
    useSummary<SequencesSummary>("sequences");

  const columns = useMemo<ColumnDef<SequenceSummary>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => (
          <Link
            to="/sequences/$sequenceKey"
            params={{ sequenceKey: keyFromUrl(row.original.sequenceUrl) }}
            className="text-primary font-medium hover:underline"
          >
            {row.original.name}
          </Link>
        ),
      },
      { accessorKey: "start", header: "Start" },
      { accessorKey: "increment", header: "Increment" },
      {
        accessorKey: "minValue",
        header: "Min",
        cell: ({ getValue }) => getValue<number | undefined>() ?? "—",
      },
      {
        accessorKey: "maxValue",
        header: "Max",
        cell: ({ getValue }) => getValue<number | undefined>() ?? "—",
      },
      { accessorKey: "cache", header: "Cache" },
      {
        accessorKey: "cycle",
        header: "Cycle",
        cell: ({ getValue }) =>
          getValue<boolean>() ? (
            <Check className="text-emerald-500 size-4" aria-label="Cycles" />
          ) : (
            <Minus
              className="text-muted-foreground size-4"
              aria-label="Does not cycle"
            />
          ),
      },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return (
      <p className="text-destructive">
        Failed to load sequences: {error.message}
      </p>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <ListOrdered className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Sequences</h1>
        <span className="text-muted-foreground">({data.sequencesCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allSequences}
        filterPlaceholder="Filter sequences…"
        initialSorting={[{ id: "name", desc: false }]}
        emptyMessage="No sequences."
      />
    </div>
  );
}
