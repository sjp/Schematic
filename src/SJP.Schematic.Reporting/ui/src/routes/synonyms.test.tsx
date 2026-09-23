import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { SynonymsPage } from "@/routes/synonyms";
import { failToLoad, renderRoute } from "@/test/utils";
import type { SynonymsSummary, SynonymSummary } from "@/types/report";

const SYNONYM: SynonymSummary = {
  name: "actor_alias",
  synonymUrl: "#/synonyms/actor_alias-1a2b3c4d",
  targetName: "dbo.actor",
  targetUrl: "#/tables/actor-d4592e62",
};

function renderSynonyms(synonyms?: SynonymsSummary) {
  return renderRoute({
    path: "/synonyms",
    component: SynonymsPage,
    data: { summaries: synonyms === undefined ? {} : { synonyms } },
  });
}

describe("SynonymsPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderSynonyms();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderSynonyms();
    expect(await screen.findByText("Failed to load synonyms: boom")).toBeInTheDocument();
  });

  it("links each row's name to its synonym detail route, derived from the hash url", async () => {
    await renderSynonyms({ synonymsCount: 1, allSynonyms: [SYNONYM] });
    expect(screen.getByRole("link", { name: "actor_alias" })).toHaveAttribute(
      "href",
      "/synonyms/actor_alias-1a2b3c4d",
    );
  });

  it("links a target the report has a page for", async () => {
    await renderSynonyms({ synonymsCount: 1, allSynonyms: [SYNONYM] });
    expect(screen.getByRole("link", { name: "dbo.actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
  });

  it("names an unknown target as plain text rather than a dead link", async () => {
    await renderSynonyms({
      synonymsCount: 1,
      allSynonyms: [{ ...SYNONYM, targetName: "other_db.thing", targetUrl: undefined }],
    });
    expect(screen.getByText("other_db.thing")).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "other_db.thing" })).not.toBeInTheDocument();
  });

  it("shows the synonyms count in the heading", async () => {
    await renderSynonyms({ synonymsCount: 2, allSynonyms: [] });
    expect(screen.getByText("(2)")).toBeInTheDocument();
    expect(screen.getByText("No synonyms.")).toBeInTheDocument();
  });
});
