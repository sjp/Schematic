import { Link } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Boxes } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { SchemaSummary, SchemasSummary } from "@/types/report";

/** Extracts the route safeKey from a `#/schemas/<key>` hash url. */
function keyFromUrl(schemaUrl: string): string {
  return schemaUrl.slice(schemaUrl.lastIndexOf("/") + 1);
}

export function SchemasPage() {
  const { data, isPending, isError, error } = useSummary<SchemasSummary>("schemas");

  const columns = useMemo<ColumnDef<AppTableFeatures, SchemaSummary>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => (
          <span className="flex items-center gap-2">
            <Link
              to="/schemas/$schemaKey"
              params={{ schemaKey: keyFromUrl(row.original.schemaUrl) }}
              className="font-medium text-primary hover:underline"
            >
              {row.original.name}
            </Link>
            {row.original.isDefault && (
              <span className="rounded bg-primary/10 px-1.5 py-0.5 text-xs text-primary">
                default
              </span>
            )}
            {row.original.isSystem && (
              <span className="rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground">
                system
              </span>
            )}
          </span>
        ),
      },
      {
        accessorKey: "owner",
        header: "Owner",
        cell: ({ getValue }) => getValue<string>() || "—",
      },
      { accessorKey: "tablesCount", header: "Tables" },
      { accessorKey: "viewsCount", header: "Views" },
      { accessorKey: "sequencesCount", header: "Sequences" },
      { accessorKey: "synonymsCount", header: "Synonyms" },
      { accessorKey: "routinesCount", header: "Routines" },
      { accessorKey: "userDefinedTypesCount", header: "Types" },
      { accessorKey: "objectCount", header: "Objects" },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load schemas: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Boxes className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">Schemas</h1>
        <span className="text-muted-foreground">({data.schemasCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allSchemas}
        filterPlaceholder="Filter schemas…"
        initialSorting={[{ id: "name", desc: false }]}
        emptyMessage="No schemas."
      />
    </div>
  );
}
