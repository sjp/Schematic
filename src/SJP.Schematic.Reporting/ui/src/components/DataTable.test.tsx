import { describe, expect, it } from "vitest";
import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import type { ColumnDef } from "@tanstack/react-table";
import { DataTable } from "@/components/DataTable";

interface Row {
  name: string;
  value: number;
}

const rows: Row[] = [
  { name: "alpha", value: 5 },
  { name: "bravo", value: 3 },
  { name: "charlie", value: 1 },
  { name: "delta", value: 4 },
  { name: "echo", value: 2 },
];

const columns: ColumnDef<Row>[] = [
  { accessorKey: "name", header: "Name" },
  { accessorKey: "value", header: "Value" },
];

function bodyRowTexts() {
  const rowEls = screen.getAllByRole("row").slice(1); // drop the header row
  return rowEls.map((row) => within(row).getAllByRole("cell")[0]!.textContent);
}

describe("DataTable", () => {
  it("renders every row when it fits on one page", () => {
    render(<DataTable columns={columns} data={rows} />);
    expect(bodyRowTexts()).toEqual([
      "alpha",
      "bravo",
      "charlie",
      "delta",
      "echo",
    ]);
    // Row-count text shows the bare count when nothing is filtered out.
    expect(screen.getByText("5", { selector: "span" })).toBeInTheDocument();
  });

  it("shows the empty message when there is no data", () => {
    render(
      <DataTable columns={columns} data={[]} emptyMessage="Nothing here." />,
    );
    expect(screen.getByText("Nothing here.")).toBeInTheDocument();
  });

  it("respects initialSorting", () => {
    render(
      <DataTable
        columns={columns}
        data={rows}
        initialSorting={[{ id: "value", desc: false }]}
      />,
    );
    expect(bodyRowTexts()).toEqual([
      "charlie",
      "echo",
      "bravo",
      "delta",
      "alpha",
    ]);
  });

  it("toggles sort order when a sortable header is clicked", async () => {
    const user = userEvent.setup();
    render(<DataTable columns={columns} data={rows} />);

    await user.click(screen.getByRole("button", { name: "Name" }));
    expect(bodyRowTexts()).toEqual([
      "alpha",
      "bravo",
      "charlie",
      "delta",
      "echo",
    ]);

    await user.click(screen.getByRole("button", { name: "Name" }));
    expect(bodyRowTexts()).toEqual([
      "echo",
      "delta",
      "charlie",
      "bravo",
      "alpha",
    ]);
  });

  it("filters rows after the debounce elapses", async () => {
    const user = userEvent.setup();
    render(<DataTable columns={columns} data={rows} />);

    await user.type(screen.getByPlaceholderText("Filter…"), "delt");

    await screen.findByText("1 of 5");
    expect(bodyRowTexts()).toEqual(["delta"]);
  });

  it("paginates and disables navigation at the boundaries", async () => {
    const user = userEvent.setup();
    render(<DataTable columns={columns} data={rows} pageSize={2} />);

    expect(bodyRowTexts()).toEqual(["alpha", "bravo"]);
    expect(screen.getByRole("button", { name: "First page" })).toBeDisabled();
    expect(
      screen.getByRole("button", { name: "Previous page" }),
    ).toBeDisabled();

    await user.click(screen.getByRole("button", { name: "Next page" }));
    expect(bodyRowTexts()).toEqual(["charlie", "delta"]);

    await user.click(screen.getByRole("button", { name: "Last page" }));
    expect(bodyRowTexts()).toEqual(["echo"]);
    expect(screen.getByRole("button", { name: "Next page" })).toBeDisabled();

    await user.click(screen.getByRole("button", { name: "First page" }));
    expect(bodyRowTexts()).toEqual(["alpha", "bravo"]);
  });
});
