import { useMemo } from 'react'
import { Link } from '@tanstack/react-router'
import { type ColumnDef } from '@tanstack/react-table'
import { Replace } from 'lucide-react'
import { DataTable } from '@/components/DataTable'
import { useSummary } from '@/hooks/useReportData'
import type { SynonymSummary, SynonymsSummary } from '@/types/report'

/** Extracts the route safeKey from a `#/synonyms/<key>` hash url. */
function keyFromUrl(synonymUrl: string): string {
  return synonymUrl.slice(synonymUrl.lastIndexOf('/') + 1)
}

export function SynonymsPage() {
  const { data, isPending, isError, error } =
    useSummary<SynonymsSummary>('synonyms')

  const columns = useMemo<ColumnDef<SynonymSummary>[]>(
    () => [
      {
        accessorKey: 'name',
        header: 'Name',
        cell: ({ row }) => (
          <Link
            to="/synonyms/$synonymKey"
            params={{ synonymKey: keyFromUrl(row.original.synonymUrl) }}
            className="text-primary font-medium hover:underline"
          >
            {row.original.name}
          </Link>
        ),
      },
      {
        accessorKey: 'targetName',
        header: 'Target',
        cell: ({ row }) =>
          row.original.targetUrl ? (
            <a
              href={row.original.targetUrl}
              className="text-primary hover:underline"
            >
              {row.original.targetName}
            </a>
          ) : (
            <span>{row.original.targetName}</span>
          ),
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
        Failed to load synonyms: {(error as Error).message}
      </p>
    )
  }

  return (
    <div className="space-y-4">
      <div className="flex items-center gap-3">
        <Replace className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Synonyms</h1>
        <span className="text-muted-foreground">({data.synonymsCount})</span>
      </div>
      <DataTable
        columns={columns}
        data={data.allSynonyms}
        filterPlaceholder="Filter synonyms…"
        initialSorting={[{ id: 'name', desc: false }]}
        emptyMessage="No synonyms."
      />
    </div>
  )
}
