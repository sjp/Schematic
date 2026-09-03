import { Link, getRouteApi } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, Minus } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { IconTooltip } from "@/components/IconTooltip";
import { IndexStatus } from "@/components/IndexStatus";
import { LintFindings } from "@/components/LintFindings";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
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
        {data.isMaterialized && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            Materialized
            {data.refreshMode && ` · refreshed ${data.refreshMode}`}
            {data.refreshMethod && ` · ${data.refreshMethod}`}
            {!data.isPopulated && " · not populated"}
          </span>
        )}
        {data.isUpdatable && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            Updatable
            {data.checkOption && ` · ${data.checkOption}`}
          </span>
        )}
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

      {data.indexesCount > 0 && (
        <Section title="Indexes" count={data.indexesCount}>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Unique</TableHead>
                <TableHead>Type</TableHead>
                <TableHead>Columns</TableHead>
                <TableHead>Included</TableHead>
                <TableHead>Filter</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.indexes.map((ix, i) => (
                <TableRow key={i}>
                  <TableCell>{ix.name || "—"}</TableCell>
                  <TableCell>
                    {ix.isUnique ? (
                      <IconTooltip label="Unique index">
                        <Check className="size-4 text-emerald-500" aria-label="Unique index" />
                      </IconTooltip>
                    ) : (
                      <IconTooltip label="Non-unique index">
                        <Minus
                          className="size-4 text-muted-foreground"
                          aria-label="Non-unique index"
                        />
                      </IconTooltip>
                    )}
                  </TableCell>
                  <TableCell>{ix.indexType || "—"}</TableCell>
                  <TableCell>{ix.columnsText}</TableCell>
                  <TableCell>{ix.includedColumnsText || "—"}</TableCell>
                  <TableCell>{ix.filterText || "—"}</TableCell>
                  <TableCell>
                    <IndexStatus {...ix} />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Section>
      )}

      {data.triggersCount > 0 && (
        <Section title="Triggers" count={data.triggersCount}>
          <div className="space-y-4">
            {data.triggers.map((tr, i) => (
              <div key={i} className="rounded-md border">
                <div className="flex flex-wrap items-center gap-x-3 border-b px-3 py-2 text-sm">
                  <span className="font-medium">{tr.triggerName}</span>
                  <span className="text-muted-foreground">
                    {tr.queryTiming} {tr.events}
                    {tr.updateColumns && ` OF ${tr.updateColumns}`}
                    {tr.granularity && ` ${tr.granularity}`}
                  </span>
                  {tr.condition && (
                    <span className="text-muted-foreground">
                      WHEN <code className="text-xs">{tr.condition}</code>
                    </span>
                  )}
                </div>
                <pre className="overflow-x-auto p-3 text-xs">{tr.definition}</pre>
              </div>
            ))}
          </div>
        </Section>
      )}

      <Section title="Definition">
        <pre className="overflow-x-auto rounded-md border p-3 text-xs">{data.definition}</pre>
      </Section>
    </div>
  );
}
