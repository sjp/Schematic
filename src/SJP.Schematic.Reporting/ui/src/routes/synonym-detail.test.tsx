import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { SynonymDetailPage } from "@/routes/synonym-detail";
import { lintMessage, lintSummary } from "@/test/lint";
import { failToLoad, renderRoute } from "@/test/utils";
import type { LintSummary, SynonymDetail } from "@/types/report";

const SYNONYM: SynonymDetail = {
  name: "actor_alias",
  synonymUrl: "#/synonyms/actor_alias-1a2b",
  targetName: "dbo.actor",
  targetUrl: "#/tables/actor-d4592e62",
};

function renderSynonym(synonym?: SynonymDetail, lint?: LintSummary) {
  return renderRoute({
    path: "/synonyms/$synonymKey",
    url: "/synonyms/actor_alias-1a2b",
    component: SynonymDetailPage,
    data: {
      details: synonym === undefined ? {} : { synonym: { "actor_alias-1a2b": synonym } },
      summaries: lint === undefined ? {} : { lint },
    },
  });
}

describe("SynonymDetailPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderSynonym();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderSynonym();
    expect(await screen.findByText("Failed to load synonym: boom")).toBeInTheDocument();
  });

  it("heads the page with the synonym's name, under a link back to the list", async () => {
    await renderSynonym(SYNONYM);
    expect(screen.getByRole("heading", { name: "actor_alias" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Synonyms" })).toHaveAttribute("href", "/synonyms");
  });

  it("shows the lint findings raised against this synonym", async () => {
    await renderSynonym(
      SYNONYM,
      lintSummary([
        lintMessage({
          message: "This synonym has no comment.",
          objectUrl: "#/synonyms/actor_alias-1a2b",
        }),
        lintMessage({ message: "Something else is wrong.", objectUrl: "#/tables/actor-1a2b" }),
      ]),
    );
    expect(screen.getByText("This synonym has no comment.")).toBeInTheDocument();
    expect(screen.queryByText("Something else is wrong.")).not.toBeInTheDocument();
  });

  it("links a target the report has a page for", async () => {
    await renderSynonym(SYNONYM);
    expect(screen.getByRole("link", { name: "dbo.actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
  });

  it("names an unknown target as plain text rather than a dead link", async () => {
    await renderSynonym({ ...SYNONYM, targetUrl: undefined });
    expect(screen.getByText("dbo.actor")).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "dbo.actor" })).not.toBeInTheDocument();
  });
});
