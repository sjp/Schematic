import { Link } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { SquareFunction } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { RoutineSummary, RoutinesSummary } from "@/types/report";

/** Extracts the route safeKey from a `#/routines/<key>` hash url. */
function keyFromUrl(routineUrl: string): string {
  return routineUrl.slice(routineUrl.lastIndexOf("/") + 1);
}

export function RoutinesPage() {
  const { data, isPending, isError, error } = useSummary<RoutinesSummary>("routines");

  const columns = useMemo<ColumnDef<AppTableFeatures, RoutineSummary>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => (
          <Link
            to="/routines/$routineKey"
            params={{ routineKey: keyFromUrl(row.original.routineUrl) }}
            className="font-medium text-primary hover:underline"
          >
            {row.original.name}
          </Link>
        ),
      },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load routines: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <SquareFunction className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">Routines</h1>
        <span className="text-muted-foreground">({data.routinesCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allRoutines}
        filterPlaceholder="Filter routines…"
        initialSorting={[{ id: "name", desc: false }]}
        emptyMessage="No routines."
      />
    </div>
  );
}
