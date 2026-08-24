import { type ColumnDef } from "@tanstack/react-table";
import { Check, ListTree, Minus } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { IconTooltip } from "@/components/IconTooltip";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { IndexRow, IndexesSummary } from "@/types/report";

export function IndexesPage() {
  const { data, isPending, isError, error } = useSummary<IndexesSummary>("indexes");

  const columns = useMemo<ColumnDef<AppTableFeatures, IndexRow>[]>(
    () => [
      {
        accessorKey: "tableName",
        header: "Table",
        cell: ({ row }) => (
          <a href={row.original.tableUrl} className="text-primary hover:underline">
            {row.original.tableName}
          </a>
        ),
      },
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => <span className="font-medium">{row.original.name}</span>,
      },
      {
        accessorKey: "isUnique",
        header: "Unique",
        cell: ({ getValue }) =>
          getValue<boolean>() ? (
            <IconTooltip label="Unique index">
              <Check className="size-4 text-emerald-500" aria-label="Unique" />
            </IconTooltip>
          ) : (
            <IconTooltip label="Non-unique index">
              <Minus className="size-4 text-muted-foreground" aria-label="Not unique" />
            </IconTooltip>
          ),
      },
      { accessorKey: "columnsText", header: "Columns" },
      { accessorKey: "includedColumnsText", header: "Included" },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load indexes: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <ListTree className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">Indexes</h1>
        <span className="text-muted-foreground">({data.indexesCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.tableIndexes}
        filterPlaceholder="Filter indexes…"
        initialSorting={[{ id: "tableName", desc: false }]}
        emptyMessage="No indexes."
      />
    </div>
  );
}
