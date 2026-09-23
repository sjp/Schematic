import { screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { RootLayout } from "@/components/layout/RootLayout";
import { lintMessage, lintSummary } from "@/test/lint";
import { renderRoute } from "@/test/utils";
import type { LintSummary } from "@/types/report";

/** Opens `url` under the layout, with a page that matches any path standing in for the route. */
function renderLayout(url = "/", lint?: LintSummary) {
  return renderRoute({
    layout: RootLayout,
    path: "$",
    url,
    component: () => <div data-testid="outlet" />,
    data: { summaries: lint === undefined ? {} : { lint } },
  });
}

/** `count` lint findings, `errors` of them errors and the rest warnings. */
function findings(count: number, errors = 0): LintSummary {
  return lintSummary(
    Array.from({ length: count }, (_, i) =>
      lintMessage({ level: i < errors ? "Error" : "Warning" }),
    ),
  );
}

/** The sidebar link for a nav entry, which is an anchor rather than a router `Link`. */
function navLink(label: string) {
  return screen.getByRole("link", { name: new RegExp(`^${label}`, "u") });
}

describe("RootLayout", () => {
  it("renders the active route through the outlet", async () => {
    await renderLayout();
    expect(screen.getByTestId("outlet")).toBeInTheDocument();
  });

  it("links every section of the report from the sidebar", async () => {
    await renderLayout();
    expect(navLink("Dashboard")).toHaveAttribute("href", "#/");
    expect(navLink("Tables")).toHaveAttribute("href", "#/tables");
    expect(navLink("Types")).toHaveAttribute("href", "#/user-defined-types");
    expect(navLink("Lint")).toHaveAttribute("href", "#/lint");
    // 15 nav entries, and nothing else in the layout is an anchor.
    expect(screen.getAllByRole("link")).toHaveLength(15);
  });

  it("marks the section the current route belongs to as active", async () => {
    await renderLayout("/tables/actor-d4592e62");
    expect(navLink("Tables")).toHaveClass("bg-sidebar-accent");
    expect(navLink("Views")).not.toHaveClass("bg-sidebar-accent");
  });

  it("treats the dashboard as active only on an exact match, not as a prefix of every route", async () => {
    const { unmount } = await renderLayout("/");
    expect(navLink("Dashboard")).toHaveClass("bg-sidebar-accent");
    unmount();

    await renderLayout("/lint");
    expect(navLink("Dashboard")).not.toHaveClass("bg-sidebar-accent");
    expect(navLink("Lint")).toHaveClass("bg-sidebar-accent");
  });

  it("badges the lint entry with the finding count", async () => {
    await renderLayout("/", findings(12));
    expect(screen.getByText("12")).toBeInTheDocument();
  });

  it("colours the badge as destructive only when a finding is an error", async () => {
    const { unmount } = await renderLayout("/", findings(3, 1));
    expect(screen.getByText("3")).toHaveClass("text-destructive");
    unmount();

    await renderLayout("/", findings(3));
    expect(screen.getByText("3")).not.toHaveClass("text-destructive");
  });

  it("leaves the badge off a clean database, and off a lint summary that has not loaded", async () => {
    const { unmount } = await renderLayout("/", lintSummary());
    expect(navLink("Lint")).toHaveTextContent(/^Lint$/u);
    unmount();

    await renderLayout("/");
    expect(navLink("Lint")).toHaveTextContent(/^Lint$/u);
  });

  it("opens the search palette from the header button", async () => {
    const user = userEvent.setup();

    await renderLayout();
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Search schema" }));
    expect(screen.getByRole("dialog")).toBeInTheDocument();
  });

  it("toggles the search palette with the ⌘K / Ctrl-K shortcut", async () => {
    const user = userEvent.setup();

    await renderLayout();

    await user.keyboard("{Control>}k{/Control}");
    expect(screen.getByRole("dialog")).toBeInTheDocument();

    await user.keyboard("{Meta>}k{/Meta}");
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("leaves an unmodified k to the page", async () => {
    const user = userEvent.setup();

    await renderLayout();

    await user.keyboard("k");
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });

  it("stops listening for the shortcut once unmounted", async () => {
    const user = userEvent.setup();

    const { unmount } = await renderLayout();
    const removeListener = vi.spyOn(document, "removeEventListener");
    unmount();
    expect(removeListener).toHaveBeenCalledWith("keydown", expect.any(Function));
    removeListener.mockRestore();

    // Nothing is left mounted to reopen.
    await user.keyboard("{Control>}k{/Control}");
    expect(screen.queryByRole("dialog")).not.toBeInTheDocument();
  });
});
