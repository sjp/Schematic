import { screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";

import { LintPage, parseLintSearch, type LintSearch } from "@/routes/lint";
import { lintMessage as message, lintRule as rule, lintSummary as summary } from "@/test/lint";
import { failToLoad, renderRoute } from "@/test/utils";
import type { LintSummary } from "@/types/report";

/** Opens the lint page at `search` (e.g. `?rule=SCHEMATIC0001`), with `lint` loaded if given. */
function renderLint(lint?: LintSummary, search = "") {
  return renderRoute({
    path: "/lint",
    url: `/lint${search}`,
    component: LintPage,
    validateSearch: parseLintSearch,
    data: { summaries: lint === undefined ? {} : { lint } },
  });
}

describe("LintPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderLint();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("network down"));

    await renderLint();
    expect(
      await screen.findByText("Failed to load lint results: network down"),
    ).toBeInTheDocument();
  });

  it("shows an empty state when there are no lint issues", async () => {
    await renderLint(summary([], []));
    expect(screen.getByText("No lint issues detected.")).toBeInTheDocument();
  });

  it("summarises the issue count separately from the rule count", async () => {
    await renderLint(
      summary(
        [message(), message({ message: "The table film has no primary key." })],
        [rule({ messageCount: 2 })],
      ),
    );
    expect(screen.getByText("2 issues across 1 rule")).toBeInTheDocument();
  });

  it("shows a tile per severity with its count", async () => {
    await renderLint(
      summary([
        message({ level: "Error" }),
        message({ level: "Warning" }),
        message({ level: "Warning" }),
      ]),
    );

    const warnings = screen.getByRole("button", { name: /warnings/iu });
    expect(within(warnings).getByText("2")).toBeInTheDocument();
    expect(
      within(screen.getByRole("button", { name: /errors/iu })).getByText("1"),
    ).toBeInTheDocument();
  });

  it("lists one row per rule by default rather than every message", async () => {
    await renderLint(
      summary([message(), message({ message: "another" })], [rule({ messageCount: 2 })]),
    );

    // The rule appears once, carrying its count — not as two separate message rows.
    const ruleLink = screen.getByRole("button", { name: "Missing primary key" });
    expect(screen.queryByText("The table actor has no primary key.")).not.toBeInTheDocument();

    const ruleRow = ruleLink.closest("tr");
    expect(ruleRow).not.toBeNull();
    expect(within(ruleRow!).getByText("2")).toBeInTheDocument();
  });

  it("selects a rule when its row is clicked", async () => {
    const { router } = await renderLint(summary([message()]));
    await userEvent.click(screen.getByRole("button", { name: "Missing primary key" }));

    await waitFor(() => {
      expect(router.state.location.search).toMatchObject({ rule: "SCHEMATIC0001" });
    });
    expect(screen.getByText("The table actor has no primary key.")).toBeInTheDocument();
  });

  it("shows only the selected rule's messages when a rule is selected", async () => {
    await renderLint(
      summary(
        [
          message(),
          message({ ruleId: "SCHEMATIC0002", ruleTitle: "Other rule", message: "other message" }),
        ],
        [rule(), rule({ ruleId: "SCHEMATIC0002", ruleTitle: "Other rule" })],
      ),
      "?rule=SCHEMATIC0002",
    );

    expect(screen.getByText("other message")).toBeInTheDocument();
    expect(screen.queryByText("The table actor has no primary key.")).not.toBeInTheDocument();
  });

  it("ignores a rule in the url that no longer exists", async () => {
    await renderLint(summary([message()]), "?view=messages&rule=SCHEMATIC9999");

    // A stale link should fall back to the full list, not an empty one.
    expect(screen.getByText("The table actor has no primary key.")).toBeInTheDocument();
  });

  it("filters messages by severity when a severity is selected", async () => {
    await renderLint(
      summary([
        message({ level: "Error", message: "an error message" }),
        message({ level: "Warning", message: "a warning message" }),
      ]),
      "?view=messages&level=Warning",
    );

    expect(screen.getByText("a warning message")).toBeInTheDocument();
    expect(screen.queryByText("an error message")).not.toBeInTheDocument();
  });

  it("clears the severity filter when the same tile is clicked again", async () => {
    const { router } = await renderLint(summary([message()]), "?level=Error");
    await userEvent.click(screen.getByRole("button", { name: /errors/iu }));

    await waitFor(() => {
      expect(router.state.location.search).not.toHaveProperty("level");
    });
  });

  it("links a message to the object that raised it", async () => {
    await renderLint(summary([message()]), "?view=messages");

    expect(screen.getByRole("link", { name: "main.actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-1",
    );
  });

  it("marks a message with no owning object as schema-wide", async () => {
    await renderLint(
      summary([message({ objectName: undefined, objectType: undefined, objectUrl: undefined })]),
      "?view=messages",
    );

    expect(screen.getByText("Schema-wide")).toBeInTheDocument();
  });

  it("offers the SARIF log written alongside the report", async () => {
    await renderLint(summary([message()]));

    expect(screen.getByRole("link", { name: /sarif/iu })).toHaveAttribute(
      "href",
      "data/lint.sarif",
    );
  });

  it("goes back to the rule list from a selected rule", async () => {
    const { router } = await renderLint(summary([message()]), "?rule=SCHEMATIC0001");
    await userEvent.click(screen.getByRole("button", { name: "All rules" }));

    await waitFor(() => {
      expect(router.state.location.search).not.toHaveProperty("rule");
    });
  });

  it("switches between the rule and message views", async () => {
    const { router } = await renderLint(summary([message()]));

    await userEvent.click(screen.getByRole("button", { name: "All messages" }));
    await waitFor(() => {
      expect(router.state.location.search).toMatchObject({ view: "messages" });
    });

    await userEvent.click(await screen.findByRole("button", { name: "By rule" }));
    await waitFor(() => {
      expect(router.state.location.search).toMatchObject({ view: "rules" });
    });
  });

  it("offers a button to clear the severity filter, but only while one is set", async () => {
    const { unmount } = await renderLint(summary([message()]));
    expect(screen.queryByRole("button", { name: "Clear severity filter" })).not.toBeInTheDocument();
    unmount();

    const { router } = await renderLint(summary([message()]), "?level=Error");
    await userEvent.click(screen.getByRole("button", { name: "Clear severity filter" }));

    await waitFor(() => {
      expect(router.state.location.search).not.toHaveProperty("level");
    });
  });

  it("offers no view tabs while a single rule is selected", async () => {
    await renderLint(summary([message()]), "?rule=SCHEMATIC0001");
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
