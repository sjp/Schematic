import {
  Columns3,
  Database,
  Eye,
  KeyRound,
  ListOrdered,
  ListTree,
  Replace,
  ShieldCheck,
  SquareFunction,
  Table2,
} from "lucide-react";

import { useSummary } from "@/hooks/useReportData";
import type { LintSummary, MainSummary } from "@/types/report";

type Stat = {
  label: string;
  value: number;
  icon: typeof Database;
  href?: string;
};

export function DashboardPage() {
  const { data, isPending, isError, error } = useSummary<MainSummary>("main");
  // Lint is a separate payload, and the dashboard is still worth showing without it, so its
  // tile is simply omitted until the query resolves rather than gating the whole page.
  const { data: lint } = useSummary<LintSummary>("lint");

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError) {
    return <p className="text-destructive">Failed to load schema summary: {error.message}</p>;
  }

  const stats: Stat[] = [
    {
      label: "Tables",
      value: data.tablesCount,
      icon: Table2,
      href: "#/tables",
    },
    { label: "Views", value: data.viewsCount, icon: Eye, href: "#/views" },
    {
      label: "Columns",
      value: data.columnsCount,
      icon: Columns3,
      href: "#/columns",
    },
    {
      label: "Constraints",
      value: data.constraintsCount,
      icon: KeyRound,
      href: "#/constraints",
    },
    {
      label: "Indexes",
      value: data.indexesCount,
      icon: ListTree,
      href: "#/indexes",
    },
    {
      label: "Sequences",
      value: data.sequencesCount,
      icon: ListOrdered,
      href: "#/sequences",
    },
    {
      label: "Synonyms",
      value: data.synonymsCount,
      icon: Replace,
      href: "#/synonyms",
    },
    {
      label: "Routines",
      value: data.routinesCount,
      icon: SquareFunction,
      href: "#/routines",
    },
  ];

  if (lint !== undefined) {
    stats.push({
      label: lint.messageCount === 1 ? "Lint issue" : "Lint issues",
      value: lint.messageCount,
      icon: ShieldCheck,
      href: "#/lint",
    });
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <Database className="size-7 text-primary" />
        <div>
          <h1 className="text-2xl font-semibold">{data.databaseName}</h1>
          <p className="text-sm text-muted-foreground">{data.databaseVersion}</p>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4 sm:grid-cols-3 lg:grid-cols-4">
        {stats.map((stat) => {
          const Icon = stat.icon;
          const card = (
            <div className="flex items-center gap-3 rounded-lg border bg-card p-4 transition-colors hover:bg-accent/40">
              <Icon className="size-6 text-muted-foreground" />
              <div>
                <div className="text-2xl font-semibold tabular-nums">{stat.value}</div>
                <div className="text-sm text-muted-foreground">{stat.label}</div>
              </div>
            </div>
          );
          return stat.href ? (
            <a key={stat.label} href={stat.href} className="block">
              {card}
            </a>
          ) : (
            <div key={stat.label}>{card}</div>
          );
        })}
      </div>

      {data.schemas.length > 0 && (
        <div className="space-y-2">
          <h2 className="text-sm font-semibold text-muted-foreground">Schemas</h2>
          <ul className="flex flex-wrap gap-2">
            {data.schemas.map((schema) => (
              <li
                key={schema.name}
                className="flex items-center gap-2 rounded-md border bg-card px-3 py-1.5 text-sm"
              >
                <span className="font-medium">{schema.name}</span>
                {schema.isDefault && (
                  <span className="rounded bg-primary/10 px-1.5 py-0.5 text-xs text-primary">
                    default
                  </span>
                )}
                {schema.isSystem && (
                  <span className="rounded bg-muted px-1.5 py-0.5 text-xs text-muted-foreground">
                    system
                  </span>
                )}
                <span className="tabular-nums text-muted-foreground">{schema.objectCount}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
