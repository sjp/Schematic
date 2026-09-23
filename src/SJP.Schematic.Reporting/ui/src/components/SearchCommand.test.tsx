import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { afterEach, describe, expect, it, vi } from "vitest";

import { SearchCommand } from "@/components/SearchCommand";
import { type ReportData, renderWithClient } from "@/test/utils";
import type { SearchSummary } from "@/types/report";

function searchData(entries: SearchSummary["entries"]): ReportData {
  return {
    summaries: { search: { entriesCount: entries.length, entries } satisfies SearchSummary },
  };
}

describe("SearchCommand", () => {
  afterEach(() => {
    window.location.hash = "";
  });

  it("renders nothing when closed", () => {
    renderWithClient(<SearchCommand open={false} onOpenChange={() => {}} />, {
      data: searchData([]),
    });
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("groups results by object type in the stable display order", () => {
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />, {
      data: searchData([
        { name: "email", objectType: "Column", url: "#/tables/actor-1" },
        { name: "actor", objectType: "Table", url: "#/tables/actor-1" },
        { name: "actor_view", objectType: "View", url: "#/views/actor-1" },
      ]),
    });

    const headings = screen.getAllByText(/^(Table|View|Column)$/u).map((el) => el.textContent);
    expect(headings).toEqual(["Table", "View", "Column"]);
  });

  it("shows an empty state when there are no entries", () => {
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />, {
      data: searchData([]),
    });
    expect(screen.getByText("No results found.")).toBeInTheDocument();
  });

  it("navigates via the hash route and closes on selection", async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn<(open: boolean) => void>();
    renderWithClient(<SearchCommand open onOpenChange={onOpenChange} />, {
      data: searchData([{ name: "actor", objectType: "Table", url: "#/tables/actor-1" }]),
    });

    await user.click(within(screen.getByRole("dialog")).getByText("actor"));

    expect(onOpenChange).toHaveBeenCalledWith(false);
    expect(window.location.hash).toBe("#/tables/actor-1");
  });

  it("renders an empty palette before the search data has loaded", () => {
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />);

    expect(screen.getByRole("dialog")).toBeInTheDocument();
    expect(screen.getByText("No results found.")).toBeInTheDocument();
  });

  it("sorts a type it has no place for after the ones it does", () => {
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />, {
      data: searchData([
        { name: "thing", objectType: "Widget", url: "#/widgets/thing-1" },
        { name: "actor", objectType: "Table", url: "#/tables/actor-1" },
      ]),
    });

    const headings = screen.getAllByText(/^(Table|Widget)$/u).map((el) => el.textContent);
    expect(headings).toEqual(["Table", "Widget"]);
  });

  it("names the owning object of a column entry", () => {
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />, {
      data: searchData([
        { name: "email", objectType: "Column", parent: "actor", url: "#/tables/actor-1" },
      ]),
    });

    expect(within(screen.getByRole("dialog")).getByText("actor")).toBeInTheDocument();
  });

  it("navigates to a url the report recorded without its leading hash", async () => {
    const user = userEvent.setup();
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />, {
      data: searchData([{ name: "actor", objectType: "Table", url: "/tables/actor-1" }]),
    });

    await user.click(within(screen.getByRole("dialog")).getByText("actor"));

    expect(window.location.hash).toBe("#/tables/actor-1");
  });
});
