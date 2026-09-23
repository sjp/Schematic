import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { OrphansPage } from "@/routes/orphans";
import { failToLoad, renderWithClient } from "@/test/utils";
import type { OrphansSummary } from "@/types/report";

function renderOrphansPage(summary?: OrphansSummary) {
  return renderWithClient(<OrphansPage />, {
    data: { summaries: summary === undefined ? {} : { orphans: summary } },
  });
}

describe("OrphansPage", () => {
  it("shows a loading indicator while pending", () => {
    renderOrphansPage();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    renderOrphansPage();
    expect(await screen.findByText("Failed to load orphan tables: boom")).toBeInTheDocument();
  });

  it("links each orphan table to its own page and shows its column count", () => {
    renderOrphansPage({
      tablesCount: 1,
      tables: [{ name: "audit_log", tableUrl: "#/tables/audit_log-1a2b", columnCount: 5 }],
    });
    expect(screen.getByRole("link", { name: "audit_log" })).toHaveAttribute(
      "href",
      "#/tables/audit_log-1a2b",
    );
    expect(screen.getByText("5")).toBeInTheDocument();
  });

  it("explains what makes a table an orphan", () => {
    renderOrphansPage({ tablesCount: 0, tables: [] });
    expect(
      screen.getByText(
        "Tables that participate in no relationships (no foreign keys to or from them).",
      ),
    ).toBeInTheDocument();
  });

  it("shows the orphan count in the heading, and says so when there are none", () => {
    renderOrphansPage({ tablesCount: 0, tables: [] });
    expect(screen.getByText("(0)")).toBeInTheDocument();
    expect(screen.getByText("No orphan tables.")).toBeInTheDocument();
  });
});
