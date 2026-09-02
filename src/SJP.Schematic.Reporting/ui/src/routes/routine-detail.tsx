import { Link, getRouteApi } from "@tanstack/react-router";

import { LintFindings } from "@/components/LintFindings";
import { useDetail } from "@/hooks/useReportData";
import type { RoutineDetail, RoutineParameter } from "@/types/report";

const routeApi = getRouteApi("/routines/$routineKey");

/** Directions read better spelled out than in the enum's casing. */
const directionLabels: Record<RoutineParameter["direction"], string> = {
  Input: "In",
  Output: "Out",
  InputOutput: "In/Out",
};

function Property({ label, value }: { label: string; value: React.ReactNode }) {
  return (
    <div className="flex flex-col gap-0.5">
      <dt className="text-sm text-muted-foreground">{label}</dt>
      <dd className="font-medium">{value}</dd>
    </div>
  );
}

function Section({
  title,
  count,
  children,
}: {
  title: string;
  count?: number;
  children: React.ReactNode;
}) {
  return (
    <section className="space-y-3">
      <h2 className="text-lg font-semibold">
        {title}
        {count !== undefined && (
          <span className="ml-2 text-sm font-normal text-muted-foreground">({count})</span>
        )}
      </h2>
      {children}
    </section>
  );
}

/**
 * Parameters are a short, fixed, already-ordered list, so they are rendered as a plain table
 * rather than through DataTable's filtering and sorting.
 */
function ParameterTable({ parameters }: { parameters: RoutineParameter[] }) {
  if (parameters.length === 0) {
    return <p className="text-muted-foreground">No parameters.</p>;
  }

  return (
    <div className="overflow-x-auto rounded-md border">
      <table className="w-full text-sm">
        <thead className="border-b bg-muted/50">
          <tr className="text-left">
            <th className="px-3 py-2 font-medium">#</th>
            <th className="px-3 py-2 font-medium">Name</th>
            <th className="px-3 py-2 font-medium">Type</th>
            <th className="px-3 py-2 font-medium">Direction</th>
            <th className="px-3 py-2 font-medium">Default</th>
          </tr>
        </thead>
        <tbody>
          {parameters.map((parameter) => (
            <tr key={parameter.ordinal} className="border-b last:border-b-0">
              <td className="px-3 py-2 text-muted-foreground">{parameter.ordinal}</td>
              <td className="px-3 py-2 font-medium">
                {parameter.parameterName ?? (
                  <span className="text-muted-foreground italic">positional</span>
                )}
              </td>
              <td className="px-3 py-2">{parameter.type}</td>
              <td className="px-3 py-2">{directionLabels[parameter.direction]}</td>
              <td className="px-3 py-2">
                {parameter.defaultValue ? (
                  <code className="text-xs">{parameter.defaultValue}</code>
                ) : null}
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </div>
  );
}

export function RoutineDetailPage() {
  const { routineKey } = routeApi.useParams();
  const { data, isPending, isError, error } = useDetail<RoutineDetail>("routine", routineKey);

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError || !data) {
    return (
      <p className="text-destructive">Failed to load routine: {error?.message ?? "not found"}</p>
    );
  }

  const isOverloaded = data.overloadsCount > 0;

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link to="/routines" className="text-sm text-muted-foreground hover:underline">
          Routines
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
      </div>

      <LintFindings objectUrl={`#/routines/${routineKey}`} />

      <dl className="grid grid-cols-2 gap-x-8 gap-y-4 sm:grid-cols-3">
        <Property label="Kind" value={data.routineType} />
        <Property label="Language" value={data.language ?? "—"} />
        <Property label="Returns" value={data.returnType ?? "—"} />
      </dl>

      {!isOverloaded && (
        <Section title="Parameters" count={data.parametersCount}>
          <ParameterTable parameters={data.parameters} />
        </Section>
      )}

      {isOverloaded && (
        <Section title="Overloads" count={data.overloadsCount}>
          <div className="space-y-6">
            {data.overloads.map((overload, index) => (
              // overloads have no identifier of their own; their order is the report's order
              <div key={index} className="space-y-3">
                <h3 className="text-sm font-semibold text-muted-foreground">
                  Overload {index + 1}
                  {overload.returnType && ` → ${overload.returnType}`}
                </h3>
                <ParameterTable parameters={overload.parameters} />
                <pre className="overflow-x-auto rounded-md border p-3 text-xs">
                  {overload.definition}
                </pre>
              </div>
            ))}
          </div>
        </Section>
      )}

      {!isOverloaded && (
        <Section title="Definition">
          <pre className="overflow-x-auto rounded-md border p-3 text-xs">{data.definition}</pre>
        </Section>
      )}
    </div>
  );
}
