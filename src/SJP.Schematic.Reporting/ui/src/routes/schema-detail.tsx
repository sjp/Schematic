import { Link, getRouteApi } from "@tanstack/react-router";
import { Eye, ListOrdered, Replace, Shapes, SquareFunction, Table2 } from "lucide-react";

import { useDetail } from "@/hooks/useReportData";
import type { SchemaDetail, SchemaObject } from "@/types/report";

const routeApi = getRouteApi("/schemas/$schemaKey");

/** The object lists a schema page shows, in the order the sidebar lists their sections. */
function objectGroups(data: SchemaDetail) {
  return [
    { label: "Tables", icon: Table2, objects: data.tables },
    { label: "Views", icon: Eye, objects: data.views },
    { label: "Sequences", icon: ListOrdered, objects: data.sequences },
    { label: "Synonyms", icon: Replace, objects: data.synonyms },
    { label: "Routines", icon: SquareFunction, objects: data.routines },
    { label: "User-Defined Types", icon: Shapes, objects: data.userDefinedTypes },
  ];
}

function ObjectGroup({
  label,
  icon: Icon,
  objects,
}: {
  label: string;
  icon: typeof Table2;
  objects: SchemaObject[];
}) {
  return (
    <section className="space-y-3">
      <h2 className="flex items-center gap-2 text-lg font-semibold">
        <Icon className="size-5 text-muted-foreground" />
        {label}
        <span className="text-sm font-normal text-muted-foreground">({objects.length})</span>
      </h2>
      <ul className="grid grid-cols-1 gap-1 sm:grid-cols-2 lg:grid-cols-3">
        {objects.map((object) => (
          <li key={object.url}>
            <a
              href={object.url}
              className="block truncate rounded-md border bg-card px-3 py-1.5 text-sm text-primary hover:bg-accent/40"
            >
              {object.name}
            </a>
          </li>
        ))}
      </ul>
    </section>
  );
}

export function SchemaDetailPage() {
  const { schemaKey } = routeApi.useParams();
  const { data, isPending, isError, error } = useDetail<SchemaDetail>("schema", schemaKey);

  if (isPending) {
    return <p className="text-muted-foreground">Loading…</p>;
  }
  if (isError || !data) {
    return (
      <p className="text-destructive">Failed to load schema: {error?.message ?? "not found"}</p>
    );
  }

  // A schema that declares nothing the report covers still has a page (the database declares it),
  // so say so rather than rendering six empty sections.
  const groups = objectGroups(data).filter((group) => group.objects.length > 0);

  return (
    <div className="space-y-8">
      <div className="flex flex-wrap items-baseline gap-x-3 gap-y-1">
        <Link to="/schemas" className="text-sm text-muted-foreground hover:underline">
          Schemas
        </Link>
        <span className="text-muted-foreground">/</span>
        <h1 className="text-2xl font-semibold">{data.name}</h1>
        <span className="text-sm text-muted-foreground">
          {data.objectCount} {data.objectCount === 1 ? "object" : "objects"}
        </span>
        {data.owner && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            owned by {data.owner}
          </span>
        )}
        {data.isDefault && (
          <span className="rounded-md bg-primary/10 px-2 py-0.5 text-xs text-primary">default</span>
        )}
        {data.isSystem && (
          <span className="rounded-md bg-muted px-2 py-0.5 text-xs text-muted-foreground">
            system
          </span>
        )}
      </div>

      {groups.length === 0 ? (
        <p className="text-muted-foreground">This schema holds no objects the report covers.</p>
      ) : (
        groups.map((group) => (
          <ObjectGroup
            key={group.label}
            label={group.label}
            icon={group.icon}
            objects={group.objects}
          />
        ))
      )}
    </div>
  );
}
