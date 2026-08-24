import { type ColumnDef } from "@tanstack/react-table";
import { Unlink } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { OrphanTable, OrphansSummary } from "@/types/report";

export function OrphansPage() {
  const { data, isPending, isError, error } = useSummary<OrphansSummary>("orphans");

  const columns = useMemo<ColumnDef<AppTableFeatures, OrphanTable>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Table",
        cell: ({ row }) => (
          <a href={row.original.tableUrl} className="font-medium text-primary hover:underline">
            {row.original.name}
          </a>
        ),
      },
      { accessorKey: "columnCount", header: "Columns" },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load orphan tables: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Unlink className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">Orphan Tables</h1>
        <span className="text-muted-foreground">({data.tablesCount})</span>
      </div>
      <p className="text-sm text-muted-foreground">
        Tables that participate in no relationships (no foreign keys to or from them).
      </p>
      <DataTable
        columns={columns}
        data={data.tables}
        filterPlaceholder="Filter orphan tables…"
        initialSorting={[{ id: "name", desc: false }]}
        emptyMessage="No orphan tables."
      />
    </div>
  );
}
