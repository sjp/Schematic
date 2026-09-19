import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { beforeEach, describe, expect, it, vi } from "vitest";

import { RootLayout } from "@/components/layout/RootLayout";
import { useSummary } from "@/hooks/useReportData";
import { loadedQuery, pendingQuery } from "@/test/queryResult";
import type { LintSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

const mockPathname = vi.fn<() => string>();

// The layout only needs the active pathname, a head slot and a child slot from the router; stub all
// three so it can render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  useRouterState: ({ select }: { select: (s: { location: { pathname: string } }) => string }) =>
    select({ location: { pathname: mockPathname() } }),
  HeadContent: () => null,
  Outlet: () => <div data-testid="outlet" />,
}));

// The real command palette pulls in cmdk and a Radix dialog; report whether it was opened instead.
vi.mock("@/components/SearchCommand", () => ({
  SearchCommand: ({ open }: { open: boolean }) => (
    <div data-testid="search-command" data-open={String(open)} />
  ),
}));

const mockUseSummary = vi.mocked(useSummary<LintSummary>);

function lintSummary(overrides: Partial<LintSummary> = {}): LintSummary {
  return {
    lintRulesCount: 0,
    lintRules: [],
    messageCount: 0,
    messages: [],
    errorCount: 0,
    warningCount: 0,
    informationCount: 0,
    objectsAffectedCount: 0,
    ...overrides,
  };
}

/** The sidebar link for a nav entry, which is an anchor rather than a router `Link`. */
function navLink(label: string) {
  return screen.getByRole("link", { name: new RegExp(`^${label}`, "u") });
}

describe("RootLayout", () => {
  beforeEach(() => {
    mockPathname.mockReturnValue("/");
  });

  it("renders the active route through the outlet", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<RootLayout />);
    expect(screen.getByTestId("outlet")).toBeInTheDocument();
  });

  it("links every section of the report from the sidebar", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<RootLayout />);
    expect(navLink("Dashboard")).toHaveAttribute("href", "#/");
    expect(navLink("Tables")).toHaveAttribute("href", "#/tables");
    expect(navLink("Types")).toHaveAttribute("href", "#/user-defined-types");
    expect(navLink("Lint")).toHaveAttribute("href", "#/lint");
    // 15 nav entries, and nothing else in the layout is an anchor.
    expect(screen.getAllByRole("link")).toHaveLength(15);
  });

  it("marks the section the current route belongs to as active", () => {
    mockUseSummary.mockReturnValue(pendingQuery());
    mockPathname.mockReturnValue("/tables/actor-d4592e62");

    render(<RootLayout />);
    expect(navLink("Tables")).toHaveClass("bg-sidebar-accent");
    expect(navLink("Views")).not.toHaveClass("bg-sidebar-accent");
  });

  it("treats the dashboard as active only on an exact match, not as a prefix of every route", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    mockPathname.mockReturnValue("/");
    const { unmount } = render(<RootLayout />);
    expect(navLink("Dashboard")).toHaveClass("bg-sidebar-accent");
    unmount();

    mockPathname.mockReturnValue("/lint");
    render(<RootLayout />);
    expect(navLink("Dashboard")).not.toHaveClass("bg-sidebar-accent");
    expect(navLink("Lint")).toHaveClass("bg-sidebar-accent");
  });

  it("badges the lint entry with the finding count", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery(lintSummary({ messageCount: 12, warningCount: 12 })),
    );

    render(<RootLayout />);
    expect(screen.getByText("12")).toBeInTheDocument();
  });

  it("colours the badge as destructive only when a finding is an error", () => {
    mockUseSummary.mockReturnValue(loadedQuery(lintSummary({ messageCount: 3, errorCount: 1 })));

    const { unmount } = render(<RootLayout />);
    expect(screen.getByText("3")).toHaveClass("text-destructive");
    unmount();

    mockUseSummary.mockReturnValue(loadedQuery(lintSummary({ messageCount: 3, warningCount: 3 })));

    render(<RootLayout />);
    expect(screen.getByText("3")).not.toHaveClass("text-destructive");
  });

  it("leaves the badge off a clean database, and off a lint summary that has not loaded", () => {
    mockUseSummary.mockReturnValue(loadedQuery(lintSummary()));

    const { unmount } = render(<RootLayout />);
    expect(navLink("Lint")).toHaveTextContent(/^Lint$/u);
    unmount();

    mockUseSummary.mockReturnValue(pendingQuery());

    render(<RootLayout />);
    expect(navLink("Lint")).toHaveTextContent(/^Lint$/u);
  });

  it("opens the search palette from the header button", async () => {
    const user = userEvent.setup();
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<RootLayout />);
    expect(screen.getByTestId("search-command")).toHaveAttribute("data-open", "false");

    await user.click(screen.getByRole("button", { name: "Search schema" }));
    expect(screen.getByTestId("search-command")).toHaveAttribute("data-open", "true");
  });

  it("toggles the search palette with the ⌘K / Ctrl-K shortcut", async () => {
    const user = userEvent.setup();
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<RootLayout />);

    await user.keyboard("{Control>}k{/Control}");
    expect(screen.getByTestId("search-command")).toHaveAttribute("data-open", "true");

    await user.keyboard("{Meta>}k{/Meta}");
    expect(screen.getByTestId("search-command")).toHaveAttribute("data-open", "false");
  });

  it("leaves an unmodified k to the page", async () => {
    const user = userEvent.setup();
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<RootLayout />);

    await user.keyboard("k");
    expect(screen.getByTestId("search-command")).toHaveAttribute("data-open", "false");
  });

  it("stops listening for the shortcut once unmounted", async () => {
    const user = userEvent.setup();
    mockUseSummary.mockReturnValue(pendingQuery());

    const { unmount } = render(<RootLayout />);
    const removeListener = vi.spyOn(document, "removeEventListener");
    unmount();
    expect(removeListener).toHaveBeenCalledWith("keydown", expect.any(Function));
    removeListener.mockRestore();

    // Nothing is left mounted to reopen.
    await user.keyboard("{Control>}k{/Control}");
    expect(screen.queryByTestId("search-command")).not.toBeInTheDocument();
  });
});
