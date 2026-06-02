import { Link, getRouteApi } from "@tanstack/react-router";
import { useDetail } from "@/hooks/useReportData";
import type { RoutineDetail } from "@/types/report";

const routeApi = getRouteApi("/routines/$routineKey");

export function RoutineDetailPage() {
  const { routineKey } = routeApi.useParams();
  const { data, isPending, isError, error } = useDetail<RoutineDetail>(
    "routine",
    routineKey,
  );

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError || !data) {
    return (
      <p className="text-destructive">
        Failed to load routine: {error?.message ?? "not found"}
      </p>
    );
  }

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link
          to="/routines"
          className="text-muted-foreground text-sm hover:underline"
        >
          Routines
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
      </div>

      <section className="space-y-3">
        <h2 className="text-lg font-semibold">Definition</h2>
        <pre className="overflow-x-auto rounded-md border p-3 text-xs">
          {data.definition}
        </pre>
      </section>
    </div>
  );
}
