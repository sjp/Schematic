import { useMemo } from 'react'
import { type ColumnDef } from '@tanstack/react-table'
import { KeyRound } from 'lucide-react'
import { DataTable } from '@/components/DataTable'
import { useSummary } from '@/hooks/useReportData'
import type {
  CheckConstraintRow,
  ConstraintsSummary,
  ForeignKeyRow,
  PrimaryKeyConstraintRow,
  UniqueKeyRow,
} from '@/types/report'

function TableLink({ name, url }: { name: string; url: string }) {
  return (
    <a href={url} className="text-primary hover:underline">
      {name}
    </a>
  )
}

function Section({
  title,
  count,
  children,
}: {
  title: string
  count: number
  children: React.ReactNode
}) {
  return (
    <section className="space-y-3">
      <h2 className="text-lg font-semibold">
        {title}
        <span className="text-muted-foreground ml-2 text-sm font-normal">({count})</span>
      </h2>
      {children}
    </section>
  )
}

export function ConstraintsPage() {
  const { data, isPending, isError, error } =
    useSummary<ConstraintsSummary>('constraints')

  const keyColumns = <T extends PrimaryKeyConstraintRow | UniqueKeyRow>() =>
    [
      {
        accessorKey: 'tableName',
        header: 'Table',
        cell: ({ row }: { row: { original: T } }) => (
          <TableLink name={row.original.tableName} url={row.original.tableUrl} />
        ),
      },
      { accessorKey: 'constraintName', header: 'Constraint' },
      { accessorKey: 'columnNames', header: 'Columns' },
    ] as ColumnDef<T>[]

  const pkColumns = useMemo(() => keyColumns<PrimaryKeyConstraintRow>(), [])
  const ukColumns = useMemo(() => keyColumns<UniqueKeyRow>(), [])

  const fkColumns = useMemo<ColumnDef<ForeignKeyRow>[]>(
    () => [
      {
        accessorKey: 'tableName',
        header: 'Table',
        cell: ({ row }) => (
          <TableLink name={row.original.tableName} url={row.original.tableUrl} />
        ),
      },
      { accessorKey: 'constraintName', header: 'Constraint' },
      { accessorKey: 'childColumnNames', header: 'Columns' },
      {
        accessorKey: 'parentTableName',
        header: 'Parent Table',
        cell: ({ row }) => (
          <TableLink
            name={row.original.parentTableName}
            url={row.original.parentTableUrl}
          />
        ),
      },
      { accessorKey: 'parentColumnNames', header: 'Parent Columns' },
      { accessorKey: 'deleteActionDescription', header: 'On Delete' },
      { accessorKey: 'updateActionDescription', header: 'On Update' },
    ],
    [],
  )

  const checkColumns = useMemo<ColumnDef<CheckConstraintRow>[]>(
    () => [
      {
        accessorKey: 'tableName',
        header: 'Table',
        cell: ({ row }) => (
          <TableLink name={row.original.tableName} url={row.original.tableUrl} />
        ),
      },
      { accessorKey: 'constraintName', header: 'Constraint' },
      {
        accessorKey: 'definition',
        header: 'Definition',
        cell: ({ getValue }) => <code className="text-xs">{getValue<string>()}</code>,
      },
    ],
    [],
  )

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>
  }
  if (isError) {
    return (
      <p className="text-destructive">
        Failed to load constraints: {(error as Error).message}
      </p>
    )
  }

  return (
    <div className="space-y-8">
      <div className="flex items-center gap-3">
        <KeyRound className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Constraints</h1>
      </div>

      <Section title="Primary Keys" count={data.primaryKeysCount}>
        <DataTable
          columns={pkColumns}
          data={data.primaryKeys}
          filterPlaceholder="Filter primary keys…"
          initialSorting={[{ id: 'tableName', desc: false }]}
          emptyMessage="No primary keys."
        />
      </Section>

      <Section title="Unique Keys" count={data.uniqueKeysCount}>
        <DataTable
          columns={ukColumns}
          data={data.uniqueKeys}
          filterPlaceholder="Filter unique keys…"
          initialSorting={[{ id: 'tableName', desc: false }]}
          emptyMessage="No unique keys."
        />
      </Section>

      <Section title="Foreign Keys" count={data.foreignKeysCount}>
        <DataTable
          columns={fkColumns}
          data={data.foreignKeys}
          filterPlaceholder="Filter foreign keys…"
          initialSorting={[{ id: 'tableName', desc: false }]}
          emptyMessage="No foreign keys."
        />
      </Section>

      <Section title="Check Constraints" count={data.checkConstraintsCount}>
        <DataTable
          columns={checkColumns}
          data={data.checkConstraints}
          filterPlaceholder="Filter check constraints…"
          initialSorting={[{ id: 'tableName', desc: false }]}
          emptyMessage="No check constraints."
        />
      </Section>
    </div>
  )
}
