import { LintLevelBadge } from "@/components/LintLevelBadge";
import { useSummary } from "@/hooks/useReportData";
import { messagesForObject } from "@/lib/lint";
import type { LintSummary } from "@/types/report";

/**
 * The lint findings raised against a single database object, for use on that object's detail
 * page.
 *
 * Reads the same `data/lint.json` the lint page does and filters it client-side rather than
 * duplicating findings into every per-object payload: the lint summary is one small cached
 * query, and a single source of truth cannot drift.
 *
 * Renders nothing at all when the object is clean, so a healthy page is not padded with an
 * empty section.
 */
export function LintFindings({ objectUrl }: { objectUrl: string }) {
  const { data, isPending, isError } = useSummary<LintSummary>("lint");

  // Lint is supplementary on a detail page: if it is still loading or failed to load, the rest
  // of the page is still worth reading, so this stays silent rather than showing an error.
  if (isPending || isError) {
    return null;
  }

  const findings = messagesForObject(data.messages, objectUrl);
  if (findings.length === 0) {
    return null;
  }

  return (
    <section className="space-y-3">
      <h2 className="text-lg font-semibold">
        Lint
        <span className="ml-2 text-sm font-normal text-muted-foreground">({findings.length})</span>
      </h2>
      <ul className="space-y-2">
        {findings.map((finding, i) => (
          <li
            key={i}
            className="flex flex-wrap items-baseline gap-x-3 gap-y-1 rounded-md border p-3"
          >
            <LintLevelBadge level={finding.level} className="self-center" />
            <span className="text-sm">{finding.message}</span>
            <a
              href={`#/lint?rule=${encodeURIComponent(finding.ruleId)}`}
              className="ml-auto text-xs text-muted-foreground hover:text-primary hover:underline"
            >
              {finding.ruleTitle}
            </a>
          </li>
        ))}
      </ul>
    </section>
  );
}
