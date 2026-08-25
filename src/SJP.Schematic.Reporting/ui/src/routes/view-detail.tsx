import { Link, getRouteApi } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, Minus } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { LintFindings } from "@/components/LintFindings";
import { useDetail } from "@/hooks/useReportData";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { ViewColumn, ViewDetail } from "@/types/report";

const routeApi = getRouteApi("/views/$viewKey");

function Section({
  title,
  count,
  children,
}: {
  title: string;
  count?: number;
  children: React.ReactNode;
}) {
  return (
    <section className="space-y-3">
      <h2 className="text-lg font-semibold">
        {title}
        {count !== undefined && (
          <span className="ml-2 text-sm font-normal text-muted-foreground">({count})</span>
        )}
      </h2>
      {children}
    </section>
  );
}

export function ViewDetailPage() {
  const { viewKey } = routeApi.useParams();
  const { data, isPending, isError, error } = useDetail<ViewDetail>("view", viewKey);

  const columns = useMemo<ColumnDef<AppTableFeatures, ViewColumn>[]>(
    () => [
      { accessorKey: "ordinal", header: "#" },
      {
        accessorKey: "columnName",
        header: "Name",
        cell: ({ row }) => <span className="font-medium">{row.original.columnName}</span>,
      },
      { accessorKey: "type", header: "Type" },
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
  if (isError || !data) {
    return <p className="text-destructive">Failed to load view: {error?.message ?? "not found"}</p>;
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link to="/views" className="text-sm text-muted-foreground hover:underline">
          Views
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
        <span className="text-sm text-muted-foreground">{data.columnsCount} columns</span>
      </div>

      <LintFindings objectUrl={`#/views/${viewKey}`} />

      <Section title="Columns" count={data.columnsCount}>
        <DataTable
          columns={columns}
          data={data.columns}
          filterPlaceholder="Filter columns…"
          initialSorting={[{ id: "ordinal", desc: false }]}
        />
      </Section>

      {data.referencedObjectsCount > 0 && (
        <Section title="Referenced Objects" count={data.referencedObjectsCount}>
          <ul className="flex flex-wrap gap-2">
            {data.referencedObjects.map((ref) => (
              <li key={ref.url}>
                <a
                  href={ref.url}
                  className="inline-block rounded-md bg-muted px-2 py-1 text-sm text-primary hover:underline"
                >
                  {ref.name}
                </a>
              </li>
            ))}
          </ul>
        </Section>
      )}

      <Section title="Definition">
        <pre className="overflow-x-auto rounded-md border p-3 text-xs">{data.definition}</pre>
      </Section>
    </div>
  );
}
