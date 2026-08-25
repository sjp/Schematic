import { describe, expect, it } from "vitest";

import { LINT_LEVELS, compareMessages, levelRank, levelStyle, messagesForObject } from "@/lib/lint";
import type { LintLevel, LintMessage } from "@/types/report";

function message(overrides: Partial<LintMessage> = {}): LintMessage {
  return {
    ruleId: "SCHEMATIC0001",
    ruleTitle: "Missing primary key",
    level: "Warning",
    message: "a message",
    objectName: "main.actor",
    objectType: "Table",
    objectUrl: "#/tables/actor-1",
    ...overrides,
  };
}

describe("levelRank", () => {
  it("ranks severities most severe first", () => {
    expect(levelRank("Error")).toBeLessThan(levelRank("Warning"));
    expect(levelRank("Warning")).toBeLessThan(levelRank("Information"));
  });

  it("ranks an unknown severity last", () => {
    // A severity added on the C# side and not yet known here must not outrank a real Error.
    expect(levelRank("Critical" as LintLevel)).toBeGreaterThan(levelRank("Information"));
  });
});

describe("levelStyle", () => {
  it("gives every known severity its own icon and colour", () => {
    const classes = LINT_LEVELS.map((level) => levelStyle(level).className);
    expect(new Set(classes).size).toBe(LINT_LEVELS.length);
  });
});

describe("compareMessages", () => {
  it("orders by severity first", () => {
    const sorted = [message({ level: "Information" }), message({ level: "Error" })].sort(
      compareMessages,
    );
    expect(sorted.map((m) => m.level)).toEqual(["Error", "Information"]);
  });

  it("orders by rule within a severity", () => {
    const sorted = [message({ ruleId: "B" }), message({ ruleId: "A" })].sort(compareMessages);
    expect(sorted.map((m) => m.ruleId)).toEqual(["A", "B"]);
  });

  it("orders by object within a rule", () => {
    const sorted = [message({ objectName: "b" }), message({ objectName: "a" })].sort(
      compareMessages,
    );
    expect(sorted.map((m) => m.objectName)).toEqual(["a", "b"]);
  });

  it("sorts a message with no object without throwing", () => {
    const sorted = [message({ objectName: undefined }), message({ objectName: "a" })].sort(
      compareMessages,
    );
    expect(sorted).toHaveLength(2);
  });
});

describe("messagesForObject", () => {
  it("keeps only the messages carrying that object's url", () => {
    const messages = [
      message({ objectUrl: "#/tables/actor-1" }),
      message({ objectUrl: "#/tables/film-2" }),
      message({ objectUrl: undefined }),
    ];

    expect(messagesForObject(messages, "#/tables/actor-1")).toHaveLength(1);
  });

  it("returns them most severe first", () => {
    const messages = [
      message({ level: "Warning", message: "warn" }),
      message({ level: "Error", message: "err" }),
    ];

    expect(messagesForObject(messages, "#/tables/actor-1").map((m) => m.message)).toEqual([
      "err",
      "warn",
    ]);
  });
});
