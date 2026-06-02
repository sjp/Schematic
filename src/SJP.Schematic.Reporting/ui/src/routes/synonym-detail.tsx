import { Link, getRouteApi } from '@tanstack/react-router'
import { useDetail } from '@/hooks/useReportData'
import type { SynonymDetail } from '@/types/report'

const routeApi = getRouteApi('/synonyms/$synonymKey')

export function SynonymDetailPage() {
  const { synonymKey } = routeApi.useParams()
  const { data, isPending, isError, error } = useDetail<SynonymDetail>(
    'synonym',
    synonymKey,
  )

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>
  }
  if (isError || !data) {
    return (
      <p className="text-destructive">
        Failed to load synonym: {(error as Error)?.message ?? 'not found'}
      </p>
    )
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link
          to="/synonyms"
          className="text-muted-foreground text-sm hover:underline"
        >
          Synonyms
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
      </div>

      <dl className="flex flex-col gap-0.5">
        <dt className="text-muted-foreground text-sm">Target</dt>
        <dd className="font-medium">
          {data.targetUrl ? (
            <a href={data.targetUrl} className="text-primary hover:underline">
              {data.targetName}
            </a>
          ) : (
            data.targetName
          )}
        </dd>
      </dl>
    </div>
  )
}
