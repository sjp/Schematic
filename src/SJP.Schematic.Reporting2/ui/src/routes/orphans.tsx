import { useMemo } from 'react'
import { type ColumnDef } from '@tanstack/react-table'
import { Unlink } from 'lucide-react'
import { DataTable } from '@/components/DataTable'
import { useSummary } from '@/hooks/useReportData'
import type { OrphanTable, OrphansSummary } from '@/types/report'

export function OrphansPage() {
  const { data, isPending, isError, error } =
    useSummary<OrphansSummary>('orphans')

  const columns = useMemo<ColumnDef<OrphanTable>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Table',
        cell: ({ row }) => (
          <a href={row.original.tableUrl} className="text-primary font-medium hover:underline">
            {row.original.name}
          </a>
        ),
      },
      { accessorKey: 'columnCount', header: 'Columns' },
      { accessorKey: 'rowCount', header: 'Rows' },
    ],
    [],
  )

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>
  }
  if (isError) {
    return (
      <p className="text-destructive">
        Failed to load orphan tables: {(error as Error).message}
      </p>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Unlink className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Orphan Tables</h1>
        <span className="text-muted-foreground">({data.tablesCount})</span>
      </div>
      <p className="text-muted-foreground text-sm">
        Tables that participate in no relationships (no foreign keys to or from them).
      </p>
      <DataTable
        columns={columns}
        data={data.tables}
        filterPlaceholder="Filter orphan tables…"
        initialSorting={[{ id: 'name', desc: false }]}
        emptyMessage="No orphan tables."
      />
    </div>
  )
}
