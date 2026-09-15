import type { ReferencedObject } from "@/types/report";

/**
 * The objects a definition references, as links into their own detail pages. Shared by every
 * detail page whose object is defined by an expression (views, routines).
 */
export function ReferencedObjectList({
  referencedObjects,
}: {
  referencedObjects: ReferencedObject[];
}) {
  return (
    <ul className="flex flex-wrap gap-2">
      {referencedObjects.map((ref) => (
        <li key={ref.url}>
          <a
            href={ref.url}
            className="inline-block rounded-md bg-muted px-2 py-1 text-sm text-primary hover:underline"
          >
            {ref.name}
          </a>
        </li>
      ))}
    </ul>
  );
}
