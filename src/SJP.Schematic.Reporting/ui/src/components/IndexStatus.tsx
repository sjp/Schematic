import { TriangleAlert } from "lucide-react";

import { IconTooltip } from "@/components/IconTooltip";

/** The parts of an index that say whether the query planner will actually use it. */
export interface IndexStatusProps {
  isEnabled: boolean;
  isValid: boolean;
  isVisible: boolean;
}

/**
 * Describes why the query planner would ignore an index: it has been disabled, it was built
 * incompletely, or it has been hidden from the planner. An index in none of those states is
 * simply usable, so nothing needs saying about it.
 */
export function getIndexStatusReasons({
  isEnabled,
  isValid,
  isVisible,
}: IndexStatusProps): string[] {
  const reasons: string[] = [];
  if (!isEnabled) {
    reasons.push("Disabled");
  }
  if (!isValid) {
    reasons.push("Invalid");
  }
  if (!isVisible) {
    reasons.push("Invisible");
  }
  return reasons;
}

/** Flags an index the query planner will not use, and renders nothing notable for a usable one. */
export function IndexStatus(props: IndexStatusProps) {
  const reasons = getIndexStatusReasons(props);
  if (reasons.length === 0) {
    return <span className="text-muted-foreground">Usable</span>;
  }

  return (
    <IconTooltip
      label={`The query planner will not use this index: ${reasons.join(", ").toLowerCase()}`}
    >
      <span className="inline-flex items-center gap-1 text-amber-600 dark:text-amber-500">
        <TriangleAlert className="size-4" aria-hidden="true" />
        {reasons.join(", ")}
      </span>
    </IconTooltip>
  );
}
