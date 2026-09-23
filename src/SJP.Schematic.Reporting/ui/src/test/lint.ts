import type { LintMessage, LintRule, LintSummary } from "@/types/report";

/** A lint finding against `actor`, with any field overridden. */
export function lintMessage(overrides: Partial<LintMessage> = {}): LintMessage {
  return {
    ruleId: "SCHEMATIC0001",
    ruleTitle: "Missing primary key",
    level: "Error",
    message: "The table actor has no primary key.",
    objectName: "main.actor",
    objectType: "Table",
    objectUrl: "#/tables/actor-1",
    ...overrides,
  };
}

/** A lint rule, with any field overridden. */
export function lintRule(overrides: Partial<LintRule> = {}): LintRule {
  return {
    ruleId: "SCHEMATIC0001",
    ruleTitle: "Missing primary key",
    level: "Error",
    messageCount: 1,
    ...overrides,
  };
}

/**
 * A `data/lint.json` payload holding `messages`, with the counts derived from them. `rules`
 * defaults to one rule covering every message.
 */
export function lintSummary(messages: LintMessage[] = [], rules?: LintRule[]): LintSummary {
  const lintRules =
    rules ?? (messages.length === 0 ? [] : [lintRule({ messageCount: messages.length })]);
  return {
    lintRules,
    lintRulesCount: lintRules.length,
    messages,
    messageCount: messages.length,
    errorCount: messages.filter((m) => m.level === "Error").length,
    warningCount: messages.filter((m) => m.level === "Warning").length,
    informationCount: messages.filter((m) => m.level === "Information").length,
    objectsAffectedCount: new Set(messages.map((m) => m.objectUrl).filter(Boolean)).size,
  };
}
