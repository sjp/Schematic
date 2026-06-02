import { useMemo } from "react";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, Columns3, Minus } from "lucide-react";
import { DataTable } from "@/components/DataTable";
import { IconTooltip } from "@/components/IconTooltip";
import { useSummary } from "@/hooks/useReportData";
import type { ColumnRow, ColumnsSummary } from "@/types/report";

const KEY_BADGES = [
  { key: "isPrimaryKey", abbr: "PK", label: "Primary key" },
  { key: "isUniqueKey", abbr: "UK", label: "Unique key" },
  { key: "isForeignKey", abbr: "FK", label: "Foreign key" },
] as const;

/** A compact key-membership badge (PK / UK / FK). */
function KeyBadges({ row }: { row: ColumnRow }) {
  const badges = KEY_BADGES.filter((b) => row[b.key]);
  if (badges.length === 0)
    return (
      <IconTooltip label="No key membership">
        <Minus className="text-muted-foreground size-4" aria-label="No key" />
      </IconTooltip>
    );
  return (
    <span className="flex gap-1">
      {badges.map((b) => (
        <IconTooltip key={b.abbr} label={b.label}>
          <span className="bg-muted rounded px-1.5 py-0.5 text-xs font-medium">
            {b.abbr}
          </span>
        </IconTooltip>
      ))}
    </span>
  );
}

export function ColumnsPage() {
  const { data, isPending, isError, error } =
    useSummary<ColumnsSummary>("columns");

  const columns = useMemo<ColumnDef<ColumnRow>[]>(
    () => [
      {
        accessorKey: "name",
        header: "Parent",
        cell: ({ row }) => (
          <a
            href={row.original.parentUrl}
            className="text-primary hover:underline"
          >
            {row.original.name}
          </a>
        ),
      },
      { accessorKey: "parentType", header: "Type" },
      { accessorKey: "ordinal", header: "#" },
      {
        accessorKey: "columnName",
        header: "Column",
        cell: ({ row }) => (
          <span className="font-medium">{row.original.columnName}</span>
        ),
      },
      { accessorKey: "type", header: "Data Type" },
      {
        accessorKey: "isNullable",
        header: "Nullable",
        cell: ({ getValue }) =>
          getValue<boolean>() ? (
            <Check className="text-emerald-500 size-4" aria-label="Nullable" />
          ) : (
            <Minus
              className="text-muted-foreground size-4"
              aria-label="Not nullable"
            />
          ),
      },
      {
        id: "keys",
        header: "Keys",
        cell: ({ row }) => <KeyBadges row={row.original} />,
      },
      {
        accessorKey: "defaultValue",
        header: "Default",
        cell: ({ getValue }) => {
          const v = getValue<string>();
          return v ? <code className="text-xs">{v}</code> : null;
        },
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
        Failed to load columns: {error.message}
      </p>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Columns3 className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Columns</h1>
        <span className="text-muted-foreground">({data.columnsCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.tableColumns}
        filterPlaceholder="Filter columns…"
        initialSorting={[{ id: "name", desc: false }]}
        emptyMessage="No columns."
      />
    </div>
  );
}
