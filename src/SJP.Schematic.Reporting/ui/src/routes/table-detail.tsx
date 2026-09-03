import { Link, getRouteApi } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { Check, EyeOff, Hash, KeyRound, Link2, Minus, ShieldCheck, Sigma } from "lucide-react";
import { useMemo, useState } from "react";

import { ConstraintStatus } from "@/components/ConstraintStatus";
import { DataTable } from "@/components/DataTable";
import { IconTooltip } from "@/components/IconTooltip";
import { IndexStatus } from "@/components/IndexStatus";
import { LintFindings } from "@/components/LintFindings";
import { RelationshipDiagram } from "@/components/RelationshipDiagram";
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
import type { AppTableFeatures } from "@/lib/tableFeatures";
import type { KeyConstraint, LinkedTable, TableColumn, TableDetail } from "@/types/report";

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
          <span className="ml-2 text-sm font-normal text-muted-foreground">({count})</span>
        )}
      </h2>
      {children}
    </section>
  );
}

/** Whether `columnName` is one of a constraint's (comma-separated) columns. */
function constraintCovers(constraint: KeyConstraint, columnName: string): boolean {
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
  const matchedUniqueKeys = uniqueKeys.filter((uk) => constraintCovers(uk, column.columnName));
  return (
    <span className="ml-1 inline-flex gap-0.5 align-middle">
      {column.isPrimaryKey && (
        <IconTooltip
          label={
            <>
              <span className="font-medium">Primary key</span>
              {primaryKey?.constraintName && <> · {primaryKey.constraintName}</>}
            </>
          }
        >
          <KeyRound className="size-3.5 text-amber-500" aria-label="Primary key" />
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
                  {matchedUniqueKeys.map((uk) => uk.constraintName || "—").join(", ")}
                </>
              )}
            </>
          }
        >
          <ShieldCheck className="size-3.5 text-sky-500" aria-label="Unique key" />
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
          <Link2 className="size-3.5 text-emerald-500" aria-label="Foreign key" />
        </IconTooltip>
      )}
      {column.isAutoIncrement && (
        <IconTooltip
          label={
            <>
              <span className="font-medium">Generated</span>
              {column.identityGeneration && <> · {column.identityGeneration}</>}
              {column.identitySequenceName && <> · {column.identitySequenceName}</>}
            </>
          }
        >
          <Hash className="size-3.5 text-violet-500" aria-label="Generated value" />
        </IconTooltip>
      )}
      {column.isComputed && (
        <IconTooltip
          label={
            <>
              <span className="font-medium">Computed</span>
              {column.computedStorage && <> · {column.computedStorage}</>}
              {column.computedDefinition && (
                <div className="opacity-90">
                  <code>{column.computedDefinition}</code>
                </div>
              )}
            </>
          }
        >
          <Sigma className="size-3.5 text-rose-500" aria-label="Computed value" />
        </IconTooltip>
      )}
      {column.isHidden && (
        <IconTooltip
          label={
            <>
              <span className="font-medium">Hidden</span>
              <div className="opacity-90">Left out of the expansion of SELECT *</div>
            </>
          }
        >
          <EyeOff className="size-3.5 text-amber-500" aria-label="Hidden column" />
        </IconTooltip>
      )}
    </span>
  );
}

export function TableDetailPage() {
  const { tableKey } = routeApi.useParams();
  const { data, isPending, isError, error } = useDetail<TableDetail>("table", tableKey);

  const columns = useMemo<ColumnDef<AppTableFeatures, TableColumn>[]>(
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
    [data?.primaryKey, data?.uniqueKeys],
  );

  const [activeDiagram, setActiveDiagram] = useState(0);

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError || !data) {
    return (
      <p className="text-destructive">Failed to load table: {error?.message ?? "not found"}</p>
    );
  }

  const diagram = data.diagrams[activeDiagram] ?? data.diagrams[0];

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link to="/tables" className="text-sm text-muted-foreground hover:underline">
          Tables
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
        <span className="text-sm text-muted-foreground">{data.columnsCount} columns</span>
        {data.kind && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            {data.kind}
          </span>
        )}
        {!data.isLogged && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            Unlogged
          </span>
        )}
        {data.collation && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            Collation · {data.collation}
          </span>
        )}
      </div>

      <LintFindings objectUrl={`#/tables/${tableKey}`} />

      <Section title="Columns" count={data.columnsCount}>
        <DataTable
          columns={columns}
          data={data.columns}
          filterPlaceholder="Filter columns…"
          initialSorting={[{ id: "ordinal", desc: false }]}
        />
      </Section>

      {data.tableSystemVersioning && (
        <Section title="System Versioning">
          <SimpleTable
            head={["History Table", "Period Start", "Period End"]}
            rows={[
              [
                <LinkedTableLink key="history" table={data.tableSystemVersioning.historyTable} />,
                data.tableSystemVersioning.periodStartColumn,
                data.tableSystemVersioning.periodEndColumn,
              ],
            ]}
          />
        </Section>
      )}

      {data.tablePartitioning && (
        <Section title="Partitioning" count={data.tablePartitioning.partitionsCount}>
          <SimpleTable
            head={["Strategy", "Key Columns", "Partitions"]}
            rows={[
              [
                data.tablePartitioning.strategy,
                data.tablePartitioning.columnNames.join(", ") || "—",
                data.tablePartitioning.partitions.length > 0 ? (
                  <span key="partitions" className="flex flex-wrap gap-x-2 gap-y-1">
                    {data.tablePartitioning.partitions.map((partition) => (
                      <LinkedTableLink key={partition.name} table={partition} />
                    ))}
                  </span>
                ) : (
                  "—"
                ),
              ],
            ]}
          />
        </Section>
      )}

      {data.primaryKeyExists && data.primaryKey && (
        <Section title="Primary Key">
          <SimpleTable
            head={["Constraint", "Columns", "Status"]}
            rows={[
              [
                data.primaryKey.constraintName || "—",
                data.primaryKey.columnNames,
                <ConstraintStatus key="status" {...data.primaryKey} />,
              ],
            ]}
          />
        </Section>
      )}

      {data.uniqueKeysCount > 0 && (
        <Section title="Unique Keys" count={data.uniqueKeysCount}>
          <SimpleTable
            head={["Constraint", "Columns", "Status"]}
            rows={data.uniqueKeys.map((uk) => [
              uk.constraintName || "—",
              uk.columnNames,
              <ConstraintStatus key="status" {...uk} />,
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
                <TableHead>Match</TableHead>
                <TableHead>Status</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {data.foreignKeys.map((fk, i) => (
                <TableRow key={i}>
                  <TableCell>{fk.constraintName || "—"}</TableCell>
                  <TableCell>{fk.childColumnNames}</TableCell>
                  <TableCell>
                    <a href={fk.parentTableUrl} className="text-primary hover:underline">
                      {fk.parentTableName}
                    </a>
                  </TableCell>
                  <TableCell>{fk.parentColumnNames}</TableCell>
                  <TableCell>{fk.deleteActionDescription}</TableCell>
                  <TableCell>{fk.updateActionDescription}</TableCell>
                  <TableCell>{fk.matchTypeDescription || "—"}</TableCell>
                  <TableCell>
                    <ConstraintStatus {...fk} />
                  </TableCell>
                </TableRow>
              ))}
            </TableBody>
          </Table>
        </Section>
      )}

      {data.checkConstraintsCount > 0 && (
        <Section title="Check Constraints" count={data.checkConstraintsCount}>
          <SimpleTable
            head={["Constraint", "Definition", "Status"]}
            rows={data.checkConstraints.map((c) => [
              c.constraintName || "—",
              c.definition,
              <ConstraintStatus key="status" {...c} />,
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

/** Renders a table named by another table's storage metadata, linked when the report has a page for it. */
function LinkedTableLink({ table }: { table: LinkedTable }) {
  if (!table.tableUrl) {
    return <span>{table.name}</span>;
  }

  return (
    <a href={table.tableUrl} className="text-primary hover:underline">
      {table.name}
    </a>
  );
}

function SimpleTable({
  head,
  rows,
}: {
  head: string[];
  rows: (string | number | React.ReactNode)[][];
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
