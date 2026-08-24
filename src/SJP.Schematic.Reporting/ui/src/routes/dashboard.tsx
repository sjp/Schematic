import {
  Columns3,
  Database,
  Eye,
  KeyRound,
  ListOrdered,
  ListTree,
  Replace,
  SquareFunction,
  Table2,
} from "lucide-react";

import { useSummary } from "@/hooks/useReportData";
import type { MainSummary } from "@/types/report";

type Stat = {
  label: string;
  value: number;
  icon: typeof Database;
  href?: string;
};

export function DashboardPage() {
  const { data, isPending, isError, error } = useSummary<MainSummary>("main");

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
        <div className="text-sm text-muted-foreground">Schemas: {data.schemas.join(", ")}</div>
      )}
    </div>
  );
}
