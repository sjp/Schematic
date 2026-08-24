import { Link } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, Eye, Minus } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { ViewSummary, ViewsSummary } from "@/types/report";

/** Extracts the route safeKey from a `#/views/<key>` hash url. */
function keyFromUrl(viewUrl: string): string {
  return viewUrl.slice(viewUrl.lastIndexOf("/") + 1);
}

export function ViewsPage() {
  const { data, isPending, isError, error } = useSummary<ViewsSummary>("views");

  const columns = useMemo<ColumnDef<AppTableFeatures, ViewSummary>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => (
          <Link
            to="/views/$viewKey"
            params={{ viewKey: keyFromUrl(row.original.viewUrl) }}
            className="font-medium text-primary hover:underline"
          >
            {row.original.name}
          </Link>
        ),
      },
      { accessorKey: "columnCount", header: "Columns" },
      {
        accessorKey: "isMaterialized",
        header: "Materialized",
        cell: ({ getValue }) =>
          getValue<boolean>() ? (
            <Check className="size-4 text-emerald-500" aria-label="Materialized" />
          ) : (
            <Minus className="size-4 text-muted-foreground" aria-label="Not materialized" />
          ),
      },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load views: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Eye className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">Views</h1>
        <span className="text-muted-foreground">({data.viewsCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allViews}
        filterPlaceholder="Filter views…"
        initialSorting={[{ id: "name", desc: false }]}
        emptyMessage="No views."
      />
    </div>
  );
}
