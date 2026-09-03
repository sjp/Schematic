import { type ColumnDef } from "@tanstack/react-table";
import { Zap } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { TriggerRow, TriggersSummary } from "@/types/report";

export function TriggersPage() {
  const { data, isPending, isError, error } = useSummary<TriggersSummary>("triggers");

  const columns = useMemo<ColumnDef<AppTableFeatures, TriggerRow>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => <span className="font-medium">{row.original.name}</span>,
      },
      {
        accessorKey: "tableName",
        header: "Table",
        cell: ({ row }) => (
          <a href={row.original.tableUrl} className="text-primary hover:underline">
            {row.original.tableName}
          </a>
        ),
      },
      { accessorKey: "queryTiming", header: "Timing" },
      { accessorKey: "events", header: "Events" },
      {
        accessorKey: "granularity",
        header: "Granularity",
        cell: ({ row }) =>
          row.original.granularity || <span className="text-muted-foreground">—</span>,
      },
      {
        accessorKey: "condition",
        header: "Condition",
        cell: ({ row }) =>
          row.original.condition ? (
            <code className="text-xs">{row.original.condition}</code>
          ) : (
            <span className="text-muted-foreground">—</span>
          ),
      },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load triggers: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Zap className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">Triggers</h1>
        <span className="text-muted-foreground">({data.triggersCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allTriggers}
        filterPlaceholder="Filter triggers…"
        initialSorting={[{ id: "tableName", desc: false }]}
        emptyMessage="No triggers."
      />
    </div>
  );
}
