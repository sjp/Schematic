import { screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ColumnsPage } from "@/routes/columns";
import { failToLoad, renderWithClient } from "@/test/utils";
import type { ColumnRow, ColumnsSummary } from "@/types/report";

function columnRow(overrides: Partial<ColumnRow>): ColumnRow {
  return {
    name: "actor",
    parentType: "Table",
    parentUrl: "#/tables/actor-1",
    ordinal: 1,
    columnName: "actor_id",
    type: "int",
    isNullable: false,
    defaultValue: "",
    isPrimaryKey: false,
    isUniqueKey: false,
    isForeignKey: false,
    ...overrides,
  };
}

function rowByColumnName(name: string) {
  return screen.getByText(name).closest("tr")!;
}

function renderColumnsPage(summary?: ColumnsSummary) {
  return renderWithClient(<ColumnsPage />, {
    data: { summaries: summary === undefined ? {} : { columns: summary } },
  });
}

describe("ColumnsPage", () => {
  it("shows a loading indicator while pending", () => {
    renderColumnsPage();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    renderColumnsPage();
    expect(await screen.findByText("Failed to load columns: boom")).toBeInTheDocument();
  });

  it("renders a PK/UK/FK badge for each key membership on the row", () => {
    renderColumnsPage({
      columnsCount: 1,
      tableColumns: [
        columnRow({
          columnName: "actor_id",
          isPrimaryKey: true,
          isForeignKey: true,
        }),
      ],
    });
    const row = rowByColumnName("actor_id");
    expect(within(row).getByText("PK")).toBeInTheDocument();
    expect(within(row).getByText("FK")).toBeInTheDocument();
    expect(within(row).queryByText("UK")).not.toBeInTheDocument();
  });

  it("shows a 'no key' icon when a column has no key membership", () => {
    renderColumnsPage({
      columnsCount: 1,
      tableColumns: [columnRow({ columnName: "description" })],
    });
    const row = rowByColumnName("description");
    expect(within(row).getByLabelText("No key")).toBeInTheDocument();
  });

  it("shows the nullable icon based on isNullable", () => {
    renderColumnsPage({
      columnsCount: 2,
      tableColumns: [
        columnRow({ columnName: "optional_col", isNullable: true }),
        columnRow({ columnName: "required_col", isNullable: false }),
      ],
    });
    expect(within(rowByColumnName("optional_col")).getByLabelText("Nullable")).toBeInTheDocument();
    expect(
      within(rowByColumnName("required_col")).getByLabelText("Not nullable"),
    ).toBeInTheDocument();
  });
});
