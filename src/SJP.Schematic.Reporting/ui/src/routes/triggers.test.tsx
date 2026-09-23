import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { TriggersPage } from "@/routes/triggers";
import { failToLoad, renderWithClient } from "@/test/utils";
import type { TriggerRow, TriggersSummary } from "@/types/report";

const TRIGGER: TriggerRow = {
  name: "ins_film",
  objectName: "film",
  objectUrl: "#/tables/film-d4592e62",
  definition: "BEGIN INSERT INTO film_text ... END",
  queryTiming: "AFTER",
  events: "INSERT",
  granularity: "FOR EACH ROW",
  condition: "NEW.rating <> 'NC-17'",
  updateColumns: "",
};

function renderTriggersPage(summary?: TriggersSummary) {
  return renderWithClient(<TriggersPage />, {
    data: { summaries: summary === undefined ? {} : { triggers: summary } },
  });
}

describe("TriggersPage", () => {
  it("shows a loading indicator while pending", () => {
    renderTriggersPage();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    renderTriggersPage();
    expect(await screen.findByText("Failed to load triggers: boom")).toBeInTheDocument();
  });

  it("links the owning object and shows the trigger's timing, events and condition", () => {
    renderTriggersPage({ triggersCount: 1, allTriggers: [TRIGGER] });
    expect(screen.getByRole("link", { name: "film" })).toHaveAttribute(
      "href",
      "#/tables/film-d4592e62",
    );
    expect(screen.getByText("ins_film")).toBeInTheDocument();
    expect(screen.getByText("AFTER")).toBeInTheDocument();
    expect(screen.getByText("INSERT")).toBeInTheDocument();
    expect(screen.getByText("FOR EACH ROW")).toBeInTheDocument();
    expect(screen.getByText("NEW.rating <> 'NC-17'")).toBeInTheDocument();
  });

  it("shows an em dash for an unreported granularity and an unconditional trigger", () => {
    renderTriggersPage({
      triggersCount: 1,
      allTriggers: [{ ...TRIGGER, granularity: "", condition: "" }],
    });
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("shows the triggers count in the heading", () => {
    renderTriggersPage({ triggersCount: 6, allTriggers: [] });
    expect(screen.getByText("(6)")).toBeInTheDocument();
    expect(screen.getByText("No triggers.")).toBeInTheDocument();
  });
});
