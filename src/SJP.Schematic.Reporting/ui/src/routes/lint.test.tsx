import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { useSummary } from "@/hooks/useReportData";
import { LintPage } from "@/routes/lint";
import type { LintSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn(),
}));

const mockUseSummary = vi.mocked(useSummary<LintSummary>);

describe("LintPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue({
      isPending: true,
      isError: false,
      data: undefined,
      error: null,
    } as never);

    render(<LintPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: true,
      data: undefined,
      error: new Error("network down"),
    } as never);

    render(<LintPage />);
    expect(
      screen.getByText("Failed to load lint results: network down"),
    ).toBeInTheDocument();
  });

  it("shows an empty state when there are no lint issues", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: { lintRulesCount: 0, lintRules: [] },
      error: null,
    } as never);

    render(<LintPage />);
    expect(screen.getByText("No lint issues detected.")).toBeInTheDocument();
  });

  it("renders rule titles grouped with their messages", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: {
        lintRulesCount: 1,
        lintRules: [
          {
            ruleTitle: "Missing primary key",
            messageCount: 2,
            messages: ["Table actor has no primary key.", "Table film too."],
          },
        ],
      },
      error: null,
    } as never);

    render(<LintPage />);
    expect(screen.getByText("Missing primary key")).toBeInTheDocument();
    expect(
      screen.getByText("Table actor has no primary key."),
    ).toBeInTheDocument();
    expect(screen.getByText("Table film too.")).toBeInTheDocument();
  });
});
