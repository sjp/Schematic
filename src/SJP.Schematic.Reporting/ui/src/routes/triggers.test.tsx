import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { TriggersPage } from "@/routes/triggers";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { TriggerRow, TriggersSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

const mockUseSummary = vi.mocked(useSummary<TriggersSummary>);

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

describe("TriggersPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<TriggersPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("boom")));

    render(<TriggersPage />);
    expect(screen.getByText("Failed to load triggers: boom")).toBeInTheDocument();
  });

  it("links the owning object and shows the trigger's timing, events and condition", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ triggersCount: 1, allTriggers: [TRIGGER] }));

    render(<TriggersPage />);
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
    mockUseSummary.mockReturnValue(
      loadedQuery({
        triggersCount: 1,
        allTriggers: [{ ...TRIGGER, granularity: "", condition: "" }],
      }),
    );

    render(<TriggersPage />);
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("shows the triggers count in the heading", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ triggersCount: 6, allTriggers: [] }));

    render(<TriggersPage />);
    expect(screen.getByText("(6)")).toBeInTheDocument();
    expect(screen.getByText("No triggers.")).toBeInTheDocument();
  });
});
