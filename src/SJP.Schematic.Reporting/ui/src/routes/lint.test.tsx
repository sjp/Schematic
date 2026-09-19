import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { LintPage, parseLintSearch, type LintSearch } from "@/routes/lint";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { LintMessage, LintRule, LintSummary } from "@/types/report";

// The page reads its severity/rule/view selection from the URL. Stubbing the route api keeps
// these tests about the page rather than about router wiring, while still letting each test
// drive the page from a given URL state and observe the navigations it requests.
const { searchState, navigateSpy } = vi.hoisted(() => ({
  searchState: { current: {} },
  navigateSpy:
    vi.fn<
      (options: { search: (prev: object) => Record<string, unknown>; replace?: boolean }) => void
    >(),
}));

vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({
    useSearch: () => searchState.current,
    useNavigate: () => navigateSpy,
  }),
}));

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

const mockUseSummary = vi.mocked(useSummary<LintSummary>);

function message(overrides: Partial<LintMessage> = {}): LintMessage {
  return {
    ruleId: "SCHEMATIC0001",
    ruleTitle: "Missing primary key",
    level: "Error",
    message: "The table actor has no primary key.",
    objectName: "main.actor",
    objectType: "Table",
    objectUrl: "#/tables/actor-1",
    ...overrides,
  };
}

function rule(overrides: Partial<LintRule> = {}): LintRule {
  return {
    ruleId: "SCHEMATIC0001",
    ruleTitle: "Missing primary key",
    level: "Error",
    messageCount: 1,
    ...overrides,
  };
}

function summary(messages: LintMessage[], rules?: LintRule[]): LintSummary {
  return {
    lintRules: rules ?? [rule({ messageCount: messages.length })],
    lintRulesCount: (rules ?? [rule()]).length,
    messages,
    messageCount: messages.length,
    errorCount: messages.filter((m) => m.level === "Error").length,
    warningCount: messages.filter((m) => m.level === "Warning").length,
    informationCount: messages.filter((m) => m.level === "Information").length,
    objectsAffectedCount: new Set(messages.map((m) => m.objectUrl).filter(Boolean)).size,
  };
}

function loaded(data: LintSummary) {
  mockUseSummary.mockReturnValue(loadedQuery(data));
}

/** The search state the page would be navigated to by its most recent `setSearch` call. */
function lastRequestedSearch(): Record<string, unknown> {
  const calls = navigateSpy.mock.calls;
  const lastCall = calls[calls.length - 1];
  if (lastCall === undefined) {
    throw new Error("Expected the page to have requested a navigation, but it did not.");
  }

  return lastCall[0].search(searchState.current);
}

describe("LintPage", () => {
  beforeEach(() => {
    searchState.current = {};
    navigateSpy.mockClear();
  });

  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<LintPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("network down")));

    render(<LintPage />);
    expect(screen.getByText("Failed to load lint results: network down")).toBeInTheDocument();
  });

  it("shows an empty state when there are no lint issues", () => {
    loaded(summary([], []));

    render(<LintPage />);
    expect(screen.getByText("No lint issues detected.")).toBeInTheDocument();
  });

  it("summarises the issue count separately from the rule count", () => {
    loaded(
      summary(
        [message(), message({ message: "The table film has no primary key." })],
        [rule({ messageCount: 2 })],
      ),
    );

    render(<LintPage />);
    expect(screen.getByText("2 issues across 1 rule")).toBeInTheDocument();
  });

  it("shows a tile per severity with its count", () => {
    loaded(
      summary([
        message({ level: "Error" }),
        message({ level: "Warning" }),
        message({ level: "Warning" }),
      ]),
    );

    render(<LintPage />);

    const warnings = screen.getByRole("button", { name: /warnings/iu });
    expect(within(warnings).getByText("2")).toBeInTheDocument();
    expect(
      within(screen.getByRole("button", { name: /errors/iu })).getByText("1"),
    ).toBeInTheDocument();
  });

  it("lists one row per rule by default rather than every message", () => {
    loaded(summary([message(), message({ message: "another" })], [rule({ messageCount: 2 })]));

    render(<LintPage />);

    // The rule appears once, carrying its count — not as two separate message rows.
    const ruleLink = screen.getByRole("button", { name: "Missing primary key" });
    expect(screen.queryByText("The table actor has no primary key.")).not.toBeInTheDocument();

    const ruleRow = ruleLink.closest("tr");
    expect(ruleRow).not.toBeNull();
    expect(within(ruleRow!).getByText("2")).toBeInTheDocument();
  });

  it("selects a rule when its row is clicked", async () => {
    loaded(summary([message()]));

    render(<LintPage />);
    await userEvent.click(screen.getByRole("button", { name: "Missing primary key" }));

    expect(lastRequestedSearch()).toMatchObject({ rule: "SCHEMATIC0001" });
  });

  it("shows only the selected rule's messages when a rule is selected", () => {
    searchState.current = { rule: "SCHEMATIC0002" };
    loaded(
      summary(
        [
          message(),
          message({ ruleId: "SCHEMATIC0002", ruleTitle: "Other rule", message: "other message" }),
        ],
        [rule(), rule({ ruleId: "SCHEMATIC0002", ruleTitle: "Other rule" })],
      ),
    );

    render(<LintPage />);

    expect(screen.getByText("other message")).toBeInTheDocument();
    expect(screen.queryByText("The table actor has no primary key.")).not.toBeInTheDocument();
  });

  it("ignores a rule in the url that no longer exists", () => {
    searchState.current = { view: "messages", rule: "SCHEMATIC9999" };
    loaded(summary([message()]));

    render(<LintPage />);

    // A stale link should fall back to the full list, not an empty one.
    expect(screen.getByText("The table actor has no primary key.")).toBeInTheDocument();
  });

  it("filters messages by severity when a severity is selected", () => {
    searchState.current = { view: "messages", level: "Warning" };
    loaded(
      summary([
        message({ level: "Error", message: "an error message" }),
        message({ level: "Warning", message: "a warning message" }),
      ]),
    );

    render(<LintPage />);

    expect(screen.getByText("a warning message")).toBeInTheDocument();
    expect(screen.queryByText("an error message")).not.toBeInTheDocument();
  });

  it("clears the severity filter when the same tile is clicked again", async () => {
    searchState.current = { level: "Error" };
    loaded(summary([message()]));

    render(<LintPage />);
    await userEvent.click(screen.getByRole("button", { name: /errors/iu }));

    expect(lastRequestedSearch()).toMatchObject({ level: undefined });
  });

  it("links a message to the object that raised it", () => {
    searchState.current = { view: "messages" };
    loaded(summary([message()]));

    render(<LintPage />);

    expect(screen.getByRole("link", { name: "main.actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-1",
    );
  });

  it("marks a message with no owning object as schema-wide", () => {
    searchState.current = { view: "messages" };
    loaded(
      summary([message({ objectName: undefined, objectType: undefined, objectUrl: undefined })]),
    );

    render(<LintPage />);

    expect(screen.getByText("Schema-wide")).toBeInTheDocument();
  });

  it("offers the SARIF log written alongside the report", () => {
    loaded(summary([message()]));

    render(<LintPage />);

    expect(screen.getByRole("link", { name: /sarif/iu })).toHaveAttribute(
      "href",
      "data/lint.sarif",
    );
  });

  it("goes back to the rule list from a selected rule", async () => {
    searchState.current = { rule: "SCHEMATIC0001" };
    loaded(summary([message()]));

    render(<LintPage />);
    await userEvent.click(screen.getByRole("button", { name: "All rules" }));

    expect(lastRequestedSearch()).toMatchObject({ rule: undefined });
  });

  it("switches between the rule and message views", async () => {
    loaded(summary([message()]));

    render(<LintPage />);

    await userEvent.click(screen.getByRole("button", { name: "All messages" }));
    expect(lastRequestedSearch()).toMatchObject({ view: "messages" });

    searchState.current = { view: "messages" };
    await userEvent.click(screen.getByRole("button", { name: "By rule" }));
    expect(lastRequestedSearch()).toMatchObject({ view: "rules" });
  });

  it("offers a button to clear the severity filter, but only while one is set", async () => {
    loaded(summary([message()]));

    const { unmount } = render(<LintPage />);
    expect(screen.queryByRole("button", { name: "Clear severity filter" })).not.toBeInTheDocument();
    unmount();

    searchState.current = { level: "Error" };
    render(<LintPage />);
    await userEvent.click(screen.getByRole("button", { name: "Clear severity filter" }));

    expect(lastRequestedSearch()).toMatchObject({ level: undefined });
  });

  it("offers no view tabs while a single rule is selected", () => {
    searchState.current = { rule: "SCHEMATIC0001" };
    loaded(summary([message()]));

    render(<LintPage />);
    expect(screen.queryByRole("button", { name: "All messages" })).not.toBeInTheDocument();
  });
});

describe("parseLintSearch", () => {
  it("defaults every field when nothing is in the url", () => {
    expect(parseLintSearch({})).toEqual<LintSearch>({
      view: undefined,
      level: undefined,
      rule: undefined,
    });
  });

  it("keeps recognised values", () => {
    expect(
      parseLintSearch({ view: "messages", level: "Warning", rule: "SCHEMATIC0009" }),
    ).toEqual<LintSearch>({
      view: "messages",
      level: "Warning",
      rule: "SCHEMATIC0009",
    });
  });

  it("drops values it does not recognise", () => {
    expect(parseLintSearch({ view: "gallery", level: "critical", rule: "" })).toEqual<LintSearch>({
      view: undefined,
      level: undefined,
      rule: undefined,
    });
  });
});
