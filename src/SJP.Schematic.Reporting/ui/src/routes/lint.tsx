import { ShieldCheck, TriangleAlert } from 'lucide-react'
import { useSummary } from '@/hooks/useReportData'
import type { LintSummary } from '@/types/report'

export function LintPage() {
  const { data, isPending, isError, error } = useSummary<LintSummary>('lint')

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>
  }
  if (isError) {
    return (
      <p className="text-destructive">
        Failed to load lint results: {(error as Error).message}
      </p>
    )
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <ShieldCheck className="text-primary size-6" />
        <h1 className="text-2xl font-semibold">Lint</h1>
        <span className="text-muted-foreground">({data.lintRulesCount})</span>
      </div>

      {data.lintRules.length === 0 ? (
        <p className="text-muted-foreground">No lint issues detected.</p>
      ) : (
        <div className="space-y-6">
          {data.lintRules.map((rule) => (
            <section key={rule.ruleTitle} className="space-y-2">
              <h2 className="flex items-center gap-2 text-lg font-semibold">
                <TriangleAlert className="size-4 text-amber-500" />
                {rule.ruleTitle}
                <span className="text-muted-foreground text-sm font-normal">
                  ({rule.messageCount})
                </span>
              </h2>
              <ul className="list-disc space-y-1 pl-10">
                {rule.messages.map((message, i) => (
                  <li key={i} className="text-muted-foreground text-sm">
                    {message}
                  </li>
                ))}
              </ul>
            </section>
          ))}
        </div>
      )}
    </div>
  )
}
