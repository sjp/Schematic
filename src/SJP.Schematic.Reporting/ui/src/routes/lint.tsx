import { getRouteApi } from "@tanstack/react-router";
import { type ColumnDef } from "@tanstack/react-table";
import { ArrowLeft, CircleCheckBig, FileJson, ShieldCheck } from "lucide-react";
import { useMemo } from "react";

import { DataTable } from "@/components/DataTable";
import { LintLevelBadge } from "@/components/LintLevelBadge";
import { Button } from "@/components/ui/button";
import { useSummary } from "@/hooks/useReportData";
import { LINT_LEVELS, compareMessages, levelRank, levelStyle } from "@/lib/lint";
import type { AppTableFeatures } from "@/lib/tableFeatures";
import { cn } from "@/lib/utils";
import type { LintLevel, LintMessage, LintRule, LintSummary } from "@/types/report";

const routeApi = getRouteApi("/lint");

/**
 * The lint page.
 *
 * A real schema produces thousands of findings, so the page is built around narrowing rather
 * than scrolling: severity tiles filter, a rule list collapses the whole set to one row per
 * rule, and every message is reachable from a filterable, paginated table. What is on screen is
 * always described by the URL, so a particular slice can be linked to and shared.
 */
export function LintPage() {
  const { data, isPending, isError, error } = useSummary<LintSummary>("lint");
  const search = routeApi.useSearch();
  const navigate = routeApi.useNavigate();

  const setSearch = (next: Partial<LintSearch>) => {
    void navigate({ search: (prev) => ({ ...prev, ...next }), replace: true });
  };

  // A `?rule=` naming no rule that fired (a stale link, or a report regenerated after the schema
  // was fixed) is ignored rather than filtering every message away to an empty table.
  const activeRule = useMemo(
    () =>
      search.rule === undefined ? undefined : data?.lintRules.find((r) => r.ruleId === search.rule),
    [data, search.rule],
  );

  const messages = useMemo(() => {
    if (data === undefined) {
      return [];
    }
    return data.messages
      .filter((m) => search.level === undefined || m.level === search.level)
      .filter((m) => activeRule === undefined || m.ruleId === activeRule.ruleId)
      .sort(compareMessages);
  }, [data, search.level, activeRule]);

  const rules = useMemo(() => {
    if (data === undefined) {
      return [];
    }
    return data.lintRules
      .filter((r) => search.level === undefined || r.level === search.level)
      .sort((a, b) => levelRank(a.level) - levelRank(b.level) || b.messageCount - a.messageCount);
  }, [data, search.level]);

  const messageColumns = useMemo<ColumnDef<AppTableFeatures, LintMessage>[]>(
    () => [
      {
        accessorKey: "level",
        header: "Severity",
        cell: ({ row }) => <LintLevelBadge level={row.original.level} />,
        // Both listings arrive sorted most-severe-first, and the severity tiles above do the
        // filtering. Sorting this column would only ever offer an alphabetical order (Error,
        // Information, Warning) that misrepresents severity, so it stays off.
        enableSorting: false,
      },
      {
        accessorKey: "ruleTitle",
        header: "Rule",
        cell: ({ row }) => (
          <button
            type="button"
            onClick={() => setSearch({ rule: row.original.ruleId })}
            className="text-left text-primary hover:underline"
          >
            {row.original.ruleTitle}
          </button>
        ),
      },
      {
        accessorKey: "objectName",
        header: "Object",
        cell: ({ row }) =>
          row.original.objectUrl ? (
            <a href={row.original.objectUrl} className="font-medium text-primary hover:underline">
              {row.original.objectName}
            </a>
          ) : (
            // A schema-wide finding belongs to no single object and has nowhere to link to.
            <span className="text-muted-foreground">Schema-wide</span>
          ),
      },
      { accessorKey: "message", header: "Message" },
    ],
    // setSearch closes over `navigate`, which is stable for the life of the route.
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  const ruleColumns = useMemo<ColumnDef<AppTableFeatures, LintRule>[]>(
    () => [
      {
        accessorKey: "level",
        header: "Severity",
        cell: ({ row }) => <LintLevelBadge level={row.original.level} />,
        // Both listings arrive sorted most-severe-first, and the severity tiles above do the
        // filtering. Sorting this column would only ever offer an alphabetical order (Error,
        // Information, Warning) that misrepresents severity, so it stays off.
        enableSorting: false,
      },
      {
        accessorKey: "ruleTitle",
        header: "Rule",
        cell: ({ row }) => (
          <button
            type="button"
            onClick={() => setSearch({ rule: row.original.ruleId })}
            className="text-left font-medium text-primary hover:underline"
          >
            {row.original.ruleTitle}
          </button>
        ),
      },
      { accessorKey: "ruleId", header: "ID" },
      {
        accessorKey: "messageCount",
        header: "Issues",
        cell: ({ row }) => <span className="tabular-nums">{row.original.messageCount}</span>,
      },
    ],
    // eslint-disable-next-line react-hooks/exhaustive-deps
    [],
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load lint results: {error.message}</p>;
  }

  if (data.messageCount === 0) {
    return (
      <div className="space-y-6">
        <LintHeader data={data} />
        <div className="flex items-center gap-3 rounded-lg border p-6 text-muted-foreground">
          <CircleCheckBig className="size-5 text-emerald-500" />
          No lint issues detected.
        </div>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <LintHeader data={data} />

      <div className="grid grid-cols-2 gap-3 sm:grid-cols-4">
        {LINT_LEVELS.map((level) => (
          <LevelTile
            key={level}
            level={level}
            count={countFor(data, level)}
            active={search.level === level}
            onToggle={() => setSearch({ level: search.level === level ? undefined : level })}
          />
        ))}
        <div className="flex items-center gap-3 rounded-lg border bg-card p-4">
          <div>
            <div className="text-2xl font-semibold tabular-nums">{data.objectsAffectedCount}</div>
            <div className="text-sm text-muted-foreground">Objects affected</div>
          </div>
        </div>
      </div>

      {activeRule ? (
        <section className="space-y-3">
          <div className="flex flex-wrap items-center gap-3">
            <Button variant="outline" size="sm" onClick={() => setSearch({ rule: undefined })}>
              <ArrowLeft className="size-4" />
              All rules
            </Button>
            <LintLevelBadge level={activeRule.level} />
            <h2 className="text-lg font-semibold">{activeRule.ruleTitle}</h2>
            <code className="text-xs text-muted-foreground">{activeRule.ruleId}</code>
          </div>
          <DataTable
            columns={messageColumns}
            data={messages}
            filterPlaceholder="Filter messages…"
            emptyMessage="No messages match the current filters."
          />
        </section>
      ) : (
        <>
          <div className="flex flex-wrap items-center gap-2">
            <ViewTab
              label="By rule"
              active={search.view !== "messages"}
              onSelect={() => setSearch({ view: "rules" })}
            />
            <ViewTab
              label="All messages"
              active={search.view === "messages"}
              onSelect={() => setSearch({ view: "messages" })}
            />
            {search.level !== undefined && (
              <Button variant="ghost" size="sm" onClick={() => setSearch({ level: undefined })}>
                Clear severity filter
              </Button>
            )}
          </div>

          {search.view === "messages" ? (
            <DataTable
              columns={messageColumns}
              data={messages}
              filterPlaceholder="Filter messages…"
              emptyMessage="No messages match the current filters."
            />
          ) : (
            <DataTable
              columns={ruleColumns}
              data={rules}
              filterPlaceholder="Filter rules…"
              emptyMessage="No rules match the current filters."
              pageSize={100}
            />
          )}
        </>
      )}
    </div>
  );
}

/** The search params the lint page keeps in the URL, so any slice of it can be linked to. */
export interface LintSearch {
  /** Which listing is shown. Defaults to the rule list. */
  view?: "rules" | "messages";
  /** Restricts both listings to one severity. */
  level?: LintLevel;
  /** Drills into a single rule's messages. */
  rule?: string;
}

/** Parses the lint page's search params, dropping anything unrecognised. */
export function parseLintSearch(search: Record<string, unknown>): LintSearch {
  const view = search.view === "messages" || search.view === "rules" ? search.view : undefined;
  const level = LINT_LEVELS.find((l) => l === search.level);
  const rule = typeof search.rule === "string" && search.rule.length > 0 ? search.rule : undefined;

  return { view, level, rule };
}

function countFor(data: LintSummary, level: LintLevel): number {
  switch (level) {
    case "Error":
      return data.errorCount;
    case "Warning":
      return data.warningCount;
    case "Information":
      return data.informationCount;
  }
}

function LintHeader({ data }: { data: LintSummary }) {
  return (
    <div className="flex flex-wrap items-center gap-3">
      <ShieldCheck className="size-6 text-primary" />
      <div className="mr-auto">
        <h1 className="text-2xl font-semibold">Lint</h1>
        <p className="text-sm text-muted-foreground">
          {data.messageCount === 0
            ? "No issues found"
            : `${data.messageCount} ${data.messageCount === 1 ? "issue" : "issues"} across ${data.lintRulesCount} ${data.lintRulesCount === 1 ? "rule" : "rules"}`}
        </p>
      </div>
      {/* Written next to data/lint.json by the report generator, for code-scanning tooling. */}
      <a
        href="data/lint.sarif"
        className="inline-flex items-center gap-1.5 text-sm text-muted-foreground hover:text-primary hover:underline"
      >
        <FileJson className="size-4" />
        SARIF
      </a>
    </div>
  );
}

function LevelTile({
  level,
  count,
  active,
  onToggle,
}: {
  level: LintLevel;
  count: number;
  active: boolean;
  onToggle: () => void;
}) {
  const { icon: Icon, className, plural } = levelStyle(level);
  return (
    <button
      type="button"
      onClick={onToggle}
      aria-pressed={active}
      className={cn(
        "flex items-center gap-3 rounded-lg border bg-card p-4 text-left transition-colors hover:bg-accent/40",
        active && "border-primary bg-accent/60",
      )}
    >
      <Icon className={cn("size-6", className)} />
      <div>
        <div className="text-2xl font-semibold tabular-nums">{count}</div>
        <div className="text-sm text-muted-foreground capitalize">{plural}</div>
      </div>
    </button>
  );
}

function ViewTab({
  label,
  active,
  onSelect,
}: {
  label: string;
  active: boolean;
  onSelect: () => void;
}) {
  return (
    <Button variant={active ? "default" : "outline"} size="sm" onClick={onSelect}>
      {label}
    </Button>
  );
}
