import { Link, getRouteApi } from '@tanstack/react-router'
import { useDetail } from '@/hooks/useReportData'
import type { SequenceDetail } from '@/types/report'

const routeApi = getRouteApi('/sequences/$sequenceKey')

function Property({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-0.5">
      <dt className="text-muted-foreground text-sm">{label}</dt>
      <dd className="font-medium">{value}</dd>
    </div>
  )
}

export function SequenceDetailPage() {
  const { sequenceKey } = routeApi.useParams()
  const { data, isPending, isError, error } = useDetail<SequenceDetail>(
    'sequence',
    sequenceKey,
  )

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>
  }
  if (isError || !data) {
    return (
      <p className="text-destructive">
        Failed to load sequence: {(error as Error)?.message ?? 'not found'}
      </p>
    )
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link
          to="/sequences"
          className="text-muted-foreground text-sm hover:underline"
        >
          Sequences
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
      </div>

      <dl className="grid grid-cols-2 gap-x-8 gap-y-4 sm:grid-cols-3">
        <Property label="Start" value={data.start} />
        <Property label="Increment" value={data.increment} />
        <Property label="Min Value" value={data.minValue ?? '—'} />
        <Property label="Max Value" value={data.maxValue ?? '—'} />
        <Property label="Cache" value={data.cache} />
        <Property label="Cycle" value={data.cycle ? 'Yes' : 'No'} />
      </dl>
    </div>
  )
}
