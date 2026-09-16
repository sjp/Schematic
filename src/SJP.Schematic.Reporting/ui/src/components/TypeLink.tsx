import { hasText } from "@/lib/utils";

/**
 * A declared type, as a link to the user-defined type's own page when the report has one. Shared by
 * every page that shows the type of a column, parameter, sequence or attribute, so a type reads the
 * same wherever it is used.
 *
 * The link is rendered from a separate `typeUrl` field rather than from the type text, so a table
 * cell still sorts and filters on the text the database reported.
 */
export function TypeLink({ type, typeUrl }: { type: string; typeUrl?: string }) {
  if (!hasText(typeUrl)) {
    return <>{type}</>;
  }

  return (
    <a href={typeUrl} className="text-primary hover:underline">
      {type}
    </a>
  );
}
