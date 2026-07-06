import { describe, expect, it, vi } from "vitest";
import { render, screen, within } from "@testing-library/react";
import { useSummary } from "@/hooks/useReportData";
import { ColumnsPage } from "@/routes/columns";
import type { ColumnRow, ColumnsSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn(),
}));

const mockUseSummary = vi.mocked(useSummary<ColumnsSummary>);

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

describe("ColumnsPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue({
      isPending: true,
      isError: false,
      data: undefined,
      error: null,
    } as never);

    render(<ColumnsPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: true,
      data: undefined,
      error: new Error("boom"),
    } as never);

    render(<ColumnsPage />);
    expect(
      screen.getByText("Failed to load columns: boom"),
    ).toBeInTheDocument();
  });

  it("renders a PK/UK/FK badge for each key membership on the row", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: {
        columnsCount: 1,
        tableColumns: [
          columnRow({
            columnName: "actor_id",
            isPrimaryKey: true,
            isForeignKey: true,
          }),
        ],
      },
      error: null,
    } as never);

    render(<ColumnsPage />);
    const row = rowByColumnName("actor_id");
    expect(within(row).getByText("PK")).toBeInTheDocument();
    expect(within(row).getByText("FK")).toBeInTheDocument();
    expect(within(row).queryByText("UK")).not.toBeInTheDocument();
  });

  it("shows a 'no key' icon when a column has no key membership", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: {
        columnsCount: 1,
        tableColumns: [columnRow({ columnName: "description" })],
      },
      error: null,
    } as never);

    render(<ColumnsPage />);
    const row = rowByColumnName("description");
    expect(within(row).getByLabelText("No key")).toBeInTheDocument();
  });

  it("shows the nullable icon based on isNullable", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: {
        columnsCount: 2,
        tableColumns: [
          columnRow({ columnName: "optional_col", isNullable: true }),
          columnRow({ columnName: "required_col", isNullable: false }),
        ],
      },
      error: null,
    } as never);

    render(<ColumnsPage />);
    expect(
      within(rowByColumnName("optional_col")).getByLabelText("Nullable"),
    ).toBeInTheDocument();
    expect(
      within(rowByColumnName("required_col")).getByLabelText("Not nullable"),
    ).toBeInTheDocument();
  });
});
