import { screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { DashboardPage } from "@/routes/dashboard";
import { renderWithClient } from "@/test/utils";
import type { LintSummary, MainSummary } from "@/types/report";

const MAIN: MainSummary = {
  databaseName: "sakila",
  databaseVersion: "SQLite 3",
  columnsCount: 10,
  constraintsCount: 2,
  indexesCount: 3,
  schemas: [
    {
      name: "main",
      schemaUrl: "#/schemas/main-1a2b3c4d",
      owner: "",
      isDefault: true,
      isSystem: false,
      tablesCount: 4,
      viewsCount: 1,
      sequencesCount: 0,
      synonymsCount: 0,
      routinesCount: 0,
      userDefinedTypesCount: 0,
      objectCount: 5,
    },
  ],
  schemasCount: 1,
  tablesCount: 4,
  viewsCount: 1,
  sequencesCount: 0,
  synonymsCount: 0,
  routinesCount: 0,
  userDefinedTypesCount: 0,
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

/** Renders the dashboard over the main summary; `lint` may be withheld to leave it loading. */
function renderDashboard({ lint }: { lint?: LintSummary }) {
  return renderWithClient(<DashboardPage />, {
    data: { summaries: lint === undefined ? { main: MAIN } : { main: MAIN, lint } },
  });
}

describe("DashboardPage", () => {
  it("shows a lint tile linking to the lint page", () => {
    renderDashboard({ lint: LINT });

    const tile = screen.getByText("Lint issues").closest("a");
    expect(tile).toHaveAttribute("href", "#/lint");
    expect(within(tile!).getByText("12")).toBeInTheDocument();
  });

  it("omits the lint tile until the lint summary has loaded", () => {
    renderDashboard({});

    // The rest of the dashboard is still worth showing without it.
    expect(screen.queryByText("Lint issues")).not.toBeInTheDocument();
    expect(screen.getByText("Tables")).toBeInTheDocument();
  });

  it("labels a single issue in the singular", () => {
    renderDashboard({ lint: { ...LINT, messageCount: 1 } });

    expect(screen.getByText("Lint issue")).toBeInTheDocument();
  });

  it("lists each schema with its object count", () => {
    renderDashboard({ lint: LINT });

    const schema = screen.getByText("main").closest("li");
    expect(within(schema!).getByText("default")).toBeInTheDocument();
    expect(within(schema!).getByText("5")).toBeInTheDocument();
  });
});
