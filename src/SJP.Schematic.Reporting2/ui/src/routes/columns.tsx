import { useMemo } from 'react'
import { type ColumnDef } from '@tanstack/react-table'
import { Check, Columns3, Minus } from 'lucide-react'
import { DataTable } from '@/components/DataTable'
import { useSummary } from '@/hooks/useReportData'
import type { ColumnRow, ColumnsSummary } from '@/types/report'

/** A compact key-membership badge (PK / UK / FK). */
function KeyBadges({ row }: { row: ColumnRow }) {
  const badges: string[] = []
  if (row.isPrimaryKey) badges.push('PK')
  if (row.isUniqueKey) badges.push('UK')
  if (row.isForeignKey) badges.push('FK')
  if (badges.length === 0) return <Minus className="text-muted-foreground size-4" />
  return (
    <span className="flex gap-1">
      {badges.map((b) => (
        <span key={b} className="bg-muted rounded px-1.5 py-0.5 text-xs font-medium">
          {b}
        </span>
      ))}
    </span>
  )
}

export function ColumnsPage() {
  const { data, isPending, isError, error } =
    useSummary<ColumnsSummary>('columns')

  const columns = useMemo<ColumnDef<ColumnRow>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Parent',
        cell: ({ row }) => (
          <a href={row.original.parentUrl} className="text-primary hover:underline">
            {row.original.name}
          </a>
        ),
      },
      { accessorKey: 'parentType', header: 'Type' },
      { accessorKey: 'ordinal', header: '#' },
      {
        accessorKey: 'columnName',
        header: 'Column',
        cell: ({ row }) => <span className="font-medium">{row.original.columnName}</span>,
      },
      { accessorKey: 'type', header: 'Data Type' },
      {
        accessorKey: 'isNullable',
        header: 'Nullable',
        cell: ({ getValue }) =>
          getValue<boolean>() ? (
            <Check className="text-emerald-500 size-4" aria-label="Nullable" />
          ) : (
            <Minus className="text-muted-foreground size-4" aria-label="Not nullable" />
          ),
      },
      {
        id: 'keys',
        header: 'Keys',
        cell: ({ row }) => <KeyBadges row={row.original} />,
      },
      {
        accessorKey: 'defaultValue',
        header: 'Default',
        cell: ({ getValue }) => {
          const v = getValue<string>()
          return v ? <code className="text-xs">{v}</code> : null
        },
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
        Failed to load columns: {(error as Error).message}
      </p>
    )
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
        initialSorting={[{ id: 'name', desc: false }]}
        emptyMessage="No columns."
      />
    </div>
  )
}
