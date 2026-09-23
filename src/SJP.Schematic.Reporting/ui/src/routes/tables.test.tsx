import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { TablesPage } from "@/routes/tables";
import { failToLoad, renderRoute } from "@/test/utils";
import type { TablesSummary } from "@/types/report";

function renderTables(tables?: TablesSummary) {
  return renderRoute({
    path: "/tables",
    component: TablesPage,
    data: { summaries: tables === undefined ? {} : { tables } },
  });
}

describe("TablesPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderTables();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderTables();
    expect(await screen.findByText("Failed to load tables: boom")).toBeInTheDocument();
  });

  it("links each row's name to its table detail route, derived from the hash url", async () => {
    await renderTables({
      tablesCount: 1,
      allTables: [
        {
          name: "actor",
          tableUrl: "#/tables/actor-d4592e62",
          parentsCount: 0,
          childrenCount: 2,
          columnCount: 4,
          kind: "",
        },
      ],
    });
    const link = screen.getByRole("link", { name: "actor" });
    expect(link).toHaveAttribute("href", "/tables/actor-d4592e62");
  });

  it("shows a row count column only when the report carries statistics", async () => {
    const table = {
      name: "actor",
      tableUrl: "#/tables/actor-d4592e62",
      parentsCount: 0,
      childrenCount: 2,
      columnCount: 4,
      kind: "",
    };

    const withoutCounts = await renderTables({ tablesCount: 1, allTables: [table] });
    expect(screen.queryByText("Rows (approx.)")).not.toBeInTheDocument();
    withoutCounts.unmount();

    await renderTables({ tablesCount: 1, allTables: [{ ...table, rowCount: 1234 }] });
    expect(screen.getByText("Rows (approx.)")).toBeInTheDocument();
    expect(screen.getByText((1234).toLocaleString())).toBeInTheDocument();
  });

  it("shows the tables count in the heading", async () => {
    await renderTables({ tablesCount: 3, allTables: [] });
    expect(screen.getByText("(3)")).toBeInTheDocument();
  });
});
