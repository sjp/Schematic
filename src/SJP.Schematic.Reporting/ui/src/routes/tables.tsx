import { Link } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Table2 } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { TableSummary, TablesSummary } from "@/types/report";

/** Extracts the route safeKey from a `#/tables/<key>` hash url. */
function keyFromUrl(tableUrl: string): string {
  return tableUrl.slice(tableUrl.lastIndexOf("/") + 1);
}

export function TablesPage() {
  const { data, isPending, isError, error } = useSummary<TablesSummary>("tables");

  const columns = useMemo<ColumnDef<AppTableFeatures, TableSummary>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => (
          <Link
            to="/tables/$tableKey"
            params={{ tableKey: keyFromUrl(row.original.tableUrl) }}
            className="font-medium text-primary hover:underline"
          >
            {row.original.name}
          </Link>
        ),
      },
      { accessorKey: "columnCount", header: "Columns" },
      { accessorKey: "parentsCount", header: "Parents" },
      { accessorKey: "childrenCount", header: "Children" },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load tables: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Table2 className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">Tables</h1>
        <span className="text-muted-foreground">({data.tablesCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allTables}
        filterPlaceholder="Filter tables…"
        initialSorting={[{ id: "name", desc: false }]}
      />
    </div>
  );
}
