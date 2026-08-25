import { levelStyle } from "@/lib/lint";
import { cn } from "@/lib/utils";
import type { LintLevel } from "@/types/report";

/**
 * A severity, shown as an icon plus its name. Used wherever a single finding or rule is listed,
 * so severity reads the same way on every page.
 */
export function LintLevelBadge({ level, className }: { level: LintLevel; className?: string }) {
  const { icon: Icon, className: levelClass } = levelStyle(level);
  return (
    <span
      className={cn("inline-flex items-center gap-1.5 whitespace-nowrap", levelClass, className)}
    >
      <Icon className="size-4 shrink-0" aria-hidden="true" />
      <span className="text-sm">{level}</span>
    </span>
  );
}
