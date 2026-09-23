import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { SequenceDetailPage } from "@/routes/sequence-detail";
import { lintMessage, lintSummary } from "@/test/lint";
import { failToLoad, renderRoute } from "@/test/utils";
import type { LintSummary, SequenceDetail } from "@/types/report";

const SEQUENCE: SequenceDetail = {
  name: "actor_actor_id_seq",
  sequenceUrl: "#/sequences/actor_id_seq-1a2b",
  type: "bigint",
  start: 1,
  increment: 2,
  minValue: 1,
  maxValue: 1000,
  cache: "20",
  cycle: true,
  isOrdered: false,
};

function renderSequence(sequence?: SequenceDetail, lint?: LintSummary) {
  return renderRoute({
    path: "/sequences/$sequenceKey",
    url: "/sequences/actor_id_seq-1a2b",
    component: SequenceDetailPage,
    data: {
      details: sequence === undefined ? {} : { sequence: { "actor_id_seq-1a2b": sequence } },
      summaries: lint === undefined ? {} : { lint },
    },
  });
}

describe("SequenceDetailPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderSequence();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderSequence();
    expect(await screen.findByText("Failed to load sequence: boom")).toBeInTheDocument();
  });

  it("heads the page with the sequence's name, under a link back to the list", async () => {
    await renderSequence(SEQUENCE);
    expect(screen.getByRole("heading", { name: "actor_actor_id_seq" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Sequences" })).toHaveAttribute("href", "/sequences");
  });

  it("shows the lint findings raised against this sequence", async () => {
    await renderSequence(
      SEQUENCE,
      lintSummary([
        lintMessage({
          message: "This sequence's cache is small.",
          objectUrl: "#/sequences/actor_id_seq-1a2b",
        }),
        lintMessage({ message: "Something else is wrong.", objectUrl: "#/tables/actor-1a2b" }),
      ]),
    );
    expect(screen.getByText("This sequence's cache is small.")).toBeInTheDocument();
    expect(screen.queryByText("Something else is wrong.")).not.toBeInTheDocument();
  });

  it("lists every generation property the sequence declares", async () => {
    await renderSequence(SEQUENCE);
    expect(screen.getByText("bigint")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("1000")).toBeInTheDocument();
    expect(screen.getByText("20")).toBeInTheDocument();
    // Cycle: yes; Ordered: no.
    expect(screen.getByText("Yes")).toBeInTheDocument();
    expect(screen.getByText("No")).toBeInTheDocument();
  });

  it("shows an em dash for an absent bound and an unreported cache", async () => {
    await renderSequence({ ...SEQUENCE, minValue: undefined, maxValue: undefined, cache: "" });
    expect(screen.getAllByText("—")).toHaveLength(3);
  });
});
