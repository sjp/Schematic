import { render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { DashboardPage } from "@/routes/dashboard";
import type { LintSummary, MainSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn(),
}));

const mockUseSummary = vi.mocked(useSummary);

const MAIN: MainSummary = {
  databaseName: "sakila",
  databaseVersion: "SQLite 3",
  columnsCount: 10,
  constraintsCount: 2,
  indexesCount: 3,
  schemas: ["main"],
  schemasCount: 1,
  tablesCount: 4,
  viewsCount: 1,
  sequencesCount: 0,
  synonymsCount: 0,
  routinesCount: 0,
};

const LINT: LintSummary = {
  lintRules: [],
  lintRulesCount: 3,
  messages: [],
  messageCount: 12,
  errorCount: 1,
  warningCount: 8,
  informationCount: 3,
  objectsAffectedCount: 4,
};

/** Answers each summary key the dashboard asks for; `lint` may be withheld to mimic loading. */
function stubSummaries({ lint }: { lint?: LintSummary }) {
  mockUseSummary.mockImplementation((key: string) => {
    if (key === "lint") {
      return {
        isPending: lint === undefined,
        isError: false,
        data: lint,
        error: null,
      } as never;
    }
    return { isPending: false, isError: false, data: MAIN, error: null } as never;
  });
}

describe("DashboardPage", () => {
  it("shows a lint tile linking to the lint page", () => {
    stubSummaries({ lint: LINT });

    render(<DashboardPage />);

    const tile = screen.getByText("Lint issues").closest("a");
    expect(tile).toHaveAttribute("href", "#/lint");
    expect(within(tile!).getByText("12")).toBeInTheDocument();
  });

  it("omits the lint tile until the lint summary has loaded", () => {
    stubSummaries({});

    render(<DashboardPage />);

    // The rest of the dashboard is still worth showing without it.
    expect(screen.queryByText("Lint issues")).not.toBeInTheDocument();
    expect(screen.getByText("Tables")).toBeInTheDocument();
  });

  it("labels a single issue in the singular", () => {
    stubSummaries({ lint: { ...LINT, messageCount: 1 } });

    render(<DashboardPage />);

    expect(screen.getByText("Lint issue")).toBeInTheDocument();
  });
});
