import { CircleAlert, Info, TriangleAlert } from "lucide-react";

import type { LintLevel, LintMessage, LintObjectType } from "@/types/report";

/**
 * Presentation rules shared by every surface that shows lint findings: the lint page, the
 * per-object sections on detail pages, the dashboard tile and the sidebar badge. Keeping the
 * severity vocabulary in one place is what stops "warning amber" meaning something different
 * on each page.
 */

/** Severities, most severe first. This is the order findings are triaged in. */
export const LINT_LEVELS: readonly LintLevel[] = ["Error", "Warning", "Information"] as const;

interface LevelStyle {
  /** Icon component for the severity. */
  icon: typeof TriangleAlert;
  /** Text colour class, used for both the icon and the label. */
  className: string;
  /** Plural noun for counts, e.g. "3 warnings". */
  plural: string;
}

const LEVEL_STYLES: Record<LintLevel, LevelStyle> = {
  Error: { icon: CircleAlert, className: "text-destructive", plural: "errors" },
  Warning: { icon: TriangleAlert, className: "text-amber-500", plural: "warnings" },
  Information: { icon: Info, className: "text-sky-500", plural: "information" },
};

export function levelStyle(level: LintLevel): LevelStyle {
  return LEVEL_STYLES[level];
}

/** Sort key that puts the most severe findings first. */
export function levelRank(level: LintLevel): number {
  const rank = LINT_LEVELS.indexOf(level);
  // An unrecognised level sorts last rather than first, so a future severity added on the C#
  // side never silently outranks a real Error.
  return rank === -1 ? LINT_LEVELS.length : rank;
}

/** Route each object type links to, used for the icon/label on a message's object column. */
export const OBJECT_TYPE_LABELS: Record<LintObjectType, string> = {
  Table: "Table",
  View: "View",
  Sequence: "Sequence",
  Synonym: "Synonym",
  Routine: "Routine",
};

/** Most severe first, then by rule, then by object — the order a reader wants to work through. */
export function compareMessages(a: LintMessage, b: LintMessage): number {
  const bySeverity = levelRank(a.level) - levelRank(b.level);
  if (bySeverity !== 0) {
    return bySeverity;
  }
  const byRule = a.ruleId.localeCompare(b.ruleId);
  if (byRule !== 0) {
    return byRule;
  }
  return (a.objectName ?? "").localeCompare(b.objectName ?? "");
}

/**
 * The findings attributed to one object. Detail pages pass their own hash route; messages carry
 * the same route in `objectUrl`, so matching on it needs no key parsing.
 */
export function messagesForObject(
  messages: readonly LintMessage[],
  objectUrl: string,
): LintMessage[] {
  return messages.filter((m) => m.objectUrl === objectUrl).sort(compareMessages);
}
