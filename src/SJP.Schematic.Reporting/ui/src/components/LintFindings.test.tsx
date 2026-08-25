import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { LintFindings } from "@/components/LintFindings";
import { useSummary } from "@/hooks/useReportData";
import type { LintMessage, LintSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn(),
}));

const mockUseSummary = vi.mocked(useSummary<LintSummary>);

function message(overrides: Partial<LintMessage> = {}): LintMessage {
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

function loaded(messages: LintMessage[]) {
  mockUseSummary.mockReturnValue({
    isPending: false,
    isError: false,
    data: {
      lintRules: [],
      lintRulesCount: 0,
      messages,
      messageCount: messages.length,
      errorCount: 0,
      warningCount: 0,
      informationCount: 0,
      objectsAffectedCount: 0,
    },
    error: null,
  } as never);
}

describe("LintFindings", () => {
  it("renders the findings raised against the given object", () => {
    loaded([message()]);

    render(<LintFindings objectUrl="#/tables/actor-1" />);

    expect(screen.getByText("Lint")).toBeInTheDocument();
    expect(screen.getByText("The table actor has no primary key.")).toBeInTheDocument();
  });

  it("ignores findings belonging to other objects", () => {
    loaded([message({ objectUrl: "#/tables/film-2", message: "a film problem" })]);

    const { container } = render(<LintFindings objectUrl="#/tables/actor-1" />);

    expect(container).toBeEmptyDOMElement();
  });

  it("renders nothing when the object is clean", () => {
    loaded([]);

    const { container } = render(<LintFindings objectUrl="#/tables/actor-1" />);

    expect(container).toBeEmptyDOMElement();
  });

  it("orders findings most severe first", () => {
    loaded([
      message({ level: "Information", message: "an informational finding" }),
      message({ level: "Error", message: "an error finding" }),
    ]);

    render(<LintFindings objectUrl="#/tables/actor-1" />);

    const rendered = screen.getAllByRole("listitem").map((li) => li.textContent ?? "");
    expect(rendered[0]).toContain("an error finding");
    expect(rendered[1]).toContain("an informational finding");
  });

  it("links each finding to its rule on the lint page", () => {
    loaded([message({ ruleId: "SCHEMATIC0009" })]);

    render(<LintFindings objectUrl="#/tables/actor-1" />);

    expect(screen.getByRole("link", { name: "Missing primary key" })).toHaveAttribute(
      "href",
      "#/lint?rule=SCHEMATIC0009",
    );
  });

  it("stays silent while the lint summary is still loading", () => {
    mockUseSummary.mockReturnValue({
      isPending: true,
      isError: false,
      data: undefined,
      error: null,
    } as never);

    const { container } = render(<LintFindings objectUrl="#/tables/actor-1" />);

    expect(container).toBeEmptyDOMElement();
  });

  it("stays silent when the lint summary fails to load", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: true,
      data: undefined,
      error: new Error("network down"),
    } as never);

    // Lint is supplementary here — a failure must not put an error banner on an
    // otherwise-working detail page.
    const { container } = render(<LintFindings objectUrl="#/tables/actor-1" />);

    expect(container).toBeEmptyDOMElement();
  });
});
