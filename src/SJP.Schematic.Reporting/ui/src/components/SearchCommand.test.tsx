import { afterEach, describe, expect, it, vi } from "vitest";
import { screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { SearchCommand } from "@/components/SearchCommand";
import { renderWithClient } from "@/test/utils";
import type { SearchSummary } from "@/types/report";

function seedSearch(entries: SearchSummary["entries"]) {
  return [
    {
      queryKey: ["summary", "search"],
      data: { entriesCount: entries.length, entries } satisfies SearchSummary,
    },
  ];
}

describe("SearchCommand", () => {
  afterEach(() => {
    window.location.hash = "";
  });

  it("renders nothing when closed", () => {
    renderWithClient(<SearchCommand open={false} onOpenChange={() => {}} />, {
      seed: seedSearch([]),
    });
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("groups results by object type in the stable display order", () => {
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />, {
      seed: seedSearch([
        { name: "email", objectType: "Column", url: "#/tables/actor-1" },
        { name: "actor", objectType: "Table", url: "#/tables/actor-1" },
        { name: "actor_view", objectType: "View", url: "#/views/actor-1" },
      ]),
    });

    const headings = screen
      .getAllByText(/^(Table|View|Column)$/)
      .map((el) => el.textContent);
    expect(headings).toEqual(["Table", "View", "Column"]);
  });

  it("shows an empty state when there are no entries", () => {
    renderWithClient(<SearchCommand open onOpenChange={() => {}} />, {
      seed: seedSearch([]),
    });
    expect(screen.getByText("No results found.")).toBeInTheDocument();
  });

  it("navigates via the hash route and closes on selection", async () => {
    const user = userEvent.setup();
    const onOpenChange = vi.fn();
    renderWithClient(<SearchCommand open onOpenChange={onOpenChange} />, {
      seed: seedSearch([
        { name: "actor", objectType: "Table", url: "#/tables/actor-1" },
      ]),
    });

    await user.click(within(screen.getByRole("dialog")).getByText("actor"));

    expect(onOpenChange).toHaveBeenCalledWith(false);
    expect(window.location.hash).toBe("#/tables/actor-1");
  });
});
