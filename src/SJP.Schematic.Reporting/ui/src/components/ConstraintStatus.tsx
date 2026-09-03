import { TriangleAlert } from "lucide-react";

import { IconTooltip } from "@/components/IconTooltip";

/** The parts of a constraint that say how, and how far, the database enforces it. */
export interface ConstraintStatusProps {
  isValidated: boolean;
  deferrabilityDescription: string;
}

/**
 * Flags a constraint the database has never verified against its existing rows, and notes when the
 * check can be deferred to the end of a transaction. A validated, non-deferrable constraint is the
 * ordinary case, so nothing needs saying about it.
 */
export function ConstraintStatus({ isValidated, deferrabilityDescription }: ConstraintStatusProps) {
  if (isValidated) {
    return deferrabilityDescription ? (
      <span className="text-muted-foreground">{deferrabilityDescription}</span>
    ) : (
      <span className="text-muted-foreground">Enforced</span>
    );
  }

  const label = deferrabilityDescription
    ? `Not validated, ${deferrabilityDescription.toLowerCase()}`
    : "Not validated";

  return (
    <IconTooltip label="The database has not verified the existing rows against this constraint, and will not rely upon it when planning queries">
      <span className="inline-flex items-center gap-1 text-amber-600 dark:text-amber-500">
        <TriangleAlert className="size-4" aria-hidden="true" />
        {label}
      </span>
    </IconTooltip>
  );
}
