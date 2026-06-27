import { useMemo, useState } from "react";
import { Link, getRouteApi } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, KeyRound, Link2, Minus, ShieldCheck } from "lucide-react";
import { DataTable } from "@/components/DataTable";
import { RelationshipDiagram } from "@/components/RelationshipDiagram";
import { IconTooltip } from "@/components/IconTooltip";
import { Button } from "@/components/ui/button";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { useDetail } from "@/hooks/useReportData";
import type { KeyConstraint, TableColumn, TableDetail } from "@/types/report";

const routeApi = getRouteApi("/tables/$tableKey");

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
          <span className="text-muted-foreground ml-2 text-sm font-normal">
            ({count})
          </span>
        )}
      </h2>
      {children}
    </section>
  );
}

/** Whether `columnName` is one of a constraint's (comma-separated) columns. */
function constraintCovers(
  constraint: KeyConstraint,
  columnName: string,
): boolean {
  return constraint.columnNames
    .split(",")
    .map((c) => c.trim())
    .includes(columnName);
}

function KeyIcons({
  column,
  primaryKey,
  uniqueKeys,
}: {
  column: TableColumn;
  primaryKey?: KeyConstraint;
  uniqueKeys: KeyConstraint[];
}) {
  const matchedUniqueKeys = uniqueKeys.filter((uk) =>
    constraintCovers(uk, column.columnName),
  );
  return (
    <span className="ml-1 inline-flex gap-0.5 align-middle">
      {column.isPrimaryKey && (
        <IconTooltip
          label={
            <>
              <span className="font-medium">Primary key</span>
              {primaryKey?.constraintName && (
                <> · {primaryKey.constraintName}</>
              )}
            </>
          }
        >
          <KeyRound
            className="text-amber-500 size-3.5"
            aria-label="Primary key"
          />
        </IconTooltip>
      )}
      {column.isUniqueKey && (
        <IconTooltip
          label={
            <>
              <span className="font-medium">Unique key</span>
              {matchedUniqueKeys.length > 0 && (
                <>
                  {" · "}
                  {matchedUniqueKeys
                    .map((uk) => uk.constraintName || "—")
                    .join(", ")}
                </>
              )}
            </>
          }
        >
          <ShieldCheck
            className="text-sky-500 size-3.5"
            aria-label="Unique key"
          />
        </IconTooltip>
      )}
      {column.isForeignKey && (
        <IconTooltip
          label={
            <div className="space-y-0.5">
              <div className="font-medium">Foreign key</div>
              {column.parentKeys.map((pk, i) => (
                <div key={i} className="opacity-90">
                  {pk.constraintDescription}
                </div>
              ))}
            </div>
          }
        >
          <Link2
            className="text-emerald-500 size-3.5"
            aria-label="Foreign key"
          />
        </IconTooltip>
      )}
    </span>
  );
}

export function TableDetailPage() {
  const { tableKey } = routeApi.useParams();
  const { data, isPending, isError, error } = useDetail<TableDetail>(
    "table",
    tableKey,
  );

  const columns = useMemo<ColumnDef<TableColumn>[]>(
    () => [
      { accessorKey: "ordinal", header: "#" },
      {
        accessorKey: "columnName",
        header: "Name",
        cell: ({ row }) => (
          <span className="font-medium">
            {row.original.columnName}
            <KeyIcons
              column={row.original}
              primaryKey={data?.primaryKey}
              uniqueKeys={data?.uniqueKeys ?? []}
            />
          </span>
        ),
      },
      { accessorKey: "type", header: "Type" },
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
        accessorKey: "defaultValue",
        header: "Default",
        cell: ({ getValue }) => {
          const v = getValue<string>();
          return v ? <code className="text-xs">{v}</code> : null;
        },
      },
    ],
    [data?.primaryKey, data?.uniqueKeys],
  );

  const [activeDiagram, setActiveDiagram] = useState(0);

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError || !data) {
    return (
      <p className="text-destructive">
        Failed to load table: {error?.message ?? "not found"}
      </p>
    );
  }

  const diagram = data.diagrams[activeDiagram] ?? data.diagrams[0];

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link
          to="/tables"
          className="text-muted-foreground text-sm hover:underline"
        >
          Tables
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
        <span className="text-muted-foreground text-sm">
          {data.rowCount.toLocaleString()} rows · {data.columnsCount} columns
        </span>
      </div>

      <Section title="Columns" count={data.columnsCount}>
        <DataTable
          columns={columns}
          data={data.columns}
          filterPlaceholder="Filter columns…"
          initialSorting={[{ id: "ordinal", desc: false }]}
        />
      </Section>

      {data.primaryKeyExists && data.primaryKey && (
        <Section title="Primary Key">
          <SimpleTable
            head={["Constraint", "Columns"]}
            rows={[
              [
                data.primaryKey.constraintName || "—",
                data.primaryKey.columnNames,
              ],
            ]}
          />
        </Section>
      )}

      {data.uniqueKeysCount > 0 && (
        <Section title="Unique Keys" count={data.uniqueKeysCount}>
          <SimpleTable
            head={["Constraint", "Columns"]}
            rows={data.uniqueKeys.map((uk) => [
              uk.constraintName || "—",
              uk.columnNames,
            ])}
          />
        </Section>
      )}

      {data.foreignKeysCount > 0 && (
        <Section title="Foreign Keys" count={data.foreignKeysCount}>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Constraint</TableHead>
                <TableHead>Columns</TableHead>
                <TableHead>Parent Table</TableHead>
                <TableHead>Parent Columns</TableHead>
                <TableHead>On Delete</TableHead>
                <TableHead>On Update</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.foreignKeys.map((fk, i) => (
                <TableRow key={i}>
                  <TableCell>{fk.constraintName || "—"}</TableCell>
                  <TableCell>{fk.childColumnNames}</TableCell>
                  <TableCell>
                    <a
                      href={fk.parentTableUrl}
                      className="text-primary hover:underline"
                    >
                      {fk.parentTableName}
                    </a>
                  </TableCell>
                  <TableCell>{fk.parentColumnNames}</TableCell>
                  <TableCell>{fk.deleteActionDescription}</TableCell>
                  <TableCell>{fk.updateActionDescription}</TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Section>
      )}

      {data.checkConstraintsCount > 0 && (
        <Section title="Check Constraints" count={data.checkConstraintsCount}>
          <SimpleTable
            head={["Constraint", "Definition"]}
            rows={data.checkConstraints.map((c) => [
              c.constraintName || "—",
              c.definition,
            ])}
          />
        </Section>
      )}

      {data.indexesCount > 0 && (
        <Section title="Indexes" count={data.indexesCount}>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Name</TableHead>
                <TableHead>Unique</TableHead>
                <TableHead>Columns</TableHead>
                <TableHead>Included</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.indexes.map((ix, i) => (
                <TableRow key={i}>
                  <TableCell>{ix.name || "—"}</TableCell>
                  <TableCell>
                    {ix.isUnique ? (
                      <IconTooltip label="Unique index">
                        <Check
                          className="text-emerald-500 size-4"
                          aria-label="Unique index"
                        />
                      </IconTooltip>
                    ) : (
                      <IconTooltip label="Non-unique index">
                        <Minus
                          className="text-muted-foreground size-4"
                          aria-label="Non-unique index"
                        />
                      </IconTooltip>
                    )}
                  </TableCell>
                  <TableCell>{ix.columnsText}</TableCell>
                  <TableCell>{ix.includedColumnsText || "—"}</TableCell>
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
                  </span>
                </div>
                <pre className="overflow-x-auto p-3 text-xs">
                  {tr.definition}
                </pre>
              </div>
            ))}
          </div>
        </Section>
      )}

      {data.diagrams.length > 0 && diagram && (
        <Section title="Relationships">
          {data.diagrams.length > 1 && (
            <div className="flex gap-2">
              {data.diagrams.map((d, i) => (
                <Button
                  key={d.containerId}
                  variant={i === activeDiagram ? "default" : "outline"}
                  size="sm"
                  onClick={() => setActiveDiagram(i)}
                >
                  {d.name}
                </Button>
              ))}
            </div>
          )}
          <RelationshipDiagram graph={diagram.graph} />
        </Section>
      )}
    </div>
  );
}

function SimpleTable({
  head,
  rows,
}: {
  head: string[];
  rows: (string | number)[][];
}) {
  return (
    <Table>
      <TableHeader>
        <TableRow>
          {head.map((h) => (
            <TableHead key={h}>{h}</TableHead>
          ))}
        </TableRow>
      </TableHeader>
      <TableBody>
        {rows.map((row, i) => (
          <TableRow key={i}>
            {row.map((cell, j) => (
              <TableCell key={j}>{cell}</TableCell>
            ))}
          </TableRow>
        ))}
      </TableBody>
    </Table>
  );
}
