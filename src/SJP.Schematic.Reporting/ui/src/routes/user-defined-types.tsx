import { Link } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, Minus, Shapes } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { useSummary } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { UserDefinedTypeSummary, UserDefinedTypesSummary } from "@/types/report";

/** Extracts the route safeKey from a `#/user-defined-types/<key>` hash url. */
function keyFromUrl(typeUrl: string): string {
  return typeUrl.slice(typeUrl.lastIndexOf("/") + 1);
}

export function UserDefinedTypesPage() {
  const { data, isPending, isError, error } =
    useSummary<UserDefinedTypesSummary>("userDefinedTypes");

  const columns = useMemo<ColumnDef<AppTableFeatures, UserDefinedTypeSummary>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Name",
        cell: ({ row }) => (
          <Link
            to="/user-defined-types/$typeKey"
            params={{ typeKey: keyFromUrl(row.original.typeUrl) }}
            className="font-medium text-primary hover:underline"
          >
            {row.original.name}
          </Link>
        ),
      },
      {
        accessorKey: "kind",
        header: "Kind",
        cell: ({ getValue }) => getValue<string>() || "—",
      },
      {
        accessorKey: "baseType",
        header: "Base Type",
        cell: ({ getValue }) => {
          const v = getValue<string>();
          return v ? <code className="text-xs">{v}</code> : "—";
        },
      },
      {
        accessorKey: "isNullable",
        header: "Nullable",
        cell: ({ getValue }) =>
          getValue<boolean>() ? (
            <Check className="size-4 text-emerald-500" aria-label="Nullable" />
          ) : (
            <Minus className="size-4 text-muted-foreground" aria-label="Not nullable" />
          ),
      },
      { accessorKey: "attributesCount", header: "Attributes" },
      { accessorKey: "enumValuesCount", header: "Values" },
    ],
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load user-defined types: {error.message}</p>;
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Shapes className="size-6 text-primary" />
        <h1 className="text-2xl font-semibold">User-Defined Types</h1>
        <span className="text-muted-foreground">({data.userDefinedTypesCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allUserDefinedTypes}
        filterPlaceholder="Filter types…"
        initialSorting={[{ id: "name", desc: false }]}
        emptyMessage="No user-defined types."
      />
    </div>
  );
}
