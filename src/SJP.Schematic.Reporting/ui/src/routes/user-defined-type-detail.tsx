import { Link, getRouteApi } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, Minus } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
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
import type { UserDefinedTypeAttribute, UserDefinedTypeDetail } from "@/types/report";

const routeApi = getRouteApi("/user-defined-types/$typeKey");

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

function Property({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-0.5">
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="font-medium">{value}</dd>
    </div>
  );
}

export function UserDefinedTypeDetailPage() {
  const { typeKey } = routeApi.useParams();
  const { data, isPending, isError, error } = useDetail<UserDefinedTypeDetail>(
    "userDefinedType",
    typeKey,
  );

  const attributeColumns = useMemo<ColumnDef<AppTableFeatures, UserDefinedTypeAttribute>[]>(
    () => [
      { accessorKey: "ordinal", header: "#" },
      {
        accessorKey: "attributeName",
        header: "Name",
        cell: ({ row }) => <span className="font-medium">{row.original.attributeName}</span>,
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
    return <p className="text-destructive">Failed to load type: {error?.message ?? "not found"}</p>;
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link to="/user-defined-types" className="text-sm text-muted-foreground hover:underline">
          User-Defined Types
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
        {data.kind && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            {data.kind}
          </span>
        )}
      </div>

      <dl className="grid grid-cols-2 gap-x-8 gap-y-4 sm:grid-cols-3">
        <Property label="Kind" value={data.kind || "—"} />
        <Property
          label="Base Type"
          value={data.baseType ? <code className="text-xs">{data.baseType}</code> : "—"}
        />
        <Property label="Nullable" value={data.isNullable ? "Yes" : "No"} />
        <Property
          label="Default"
          value={data.defaultValue ? <code className="text-xs">{data.defaultValue}</code> : "—"}
        />
      </dl>

      {data.enumValuesCount > 0 && (
        <Section title="Values" count={data.enumValuesCount}>
          <ul className="flex flex-wrap gap-2">
            {data.enumValues.map((value) => (
              <li key={value} className="rounded-md border bg-card px-2 py-1 text-sm">
                <code className="text-xs">{value}</code>
              </li>
            ))}
          </ul>
        </Section>
      )}

      {data.attributesCount > 0 && (
        <Section title="Attributes" count={data.attributesCount}>
          <DataTable
            columns={attributeColumns}
            data={data.attributes}
            filterPlaceholder="Filter attributes…"
            initialSorting={[{ id: "ordinal", desc: false }]}
            emptyMessage="No attributes."
          />
        </Section>
      )}

      {data.checksCount > 0 && (
        <Section title="Check Constraints" count={data.checksCount}>
          <div className="overflow-x-auto rounded-md border">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead>Name</TableHead>
                  <TableHead>Definition</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {data.checks.map((check, i) => (
                  <TableRow key={`${check.constraintName}:${i}`}>
                    <TableCell className="font-medium">{check.constraintName || "—"}</TableCell>
                    <TableCell>
                      <code className="text-xs">{check.definition}</code>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>
        </Section>
      )}

      {data.definition && (
        <Section title="Definition">
          <pre className="overflow-x-auto rounded-md border bg-muted/40 p-4 text-xs">
            <code>{data.definition}</code>
          </pre>
        </Section>
      )}
    </div>
  );
}
