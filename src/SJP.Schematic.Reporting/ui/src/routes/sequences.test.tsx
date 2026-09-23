import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { SequencesPage } from "@/routes/sequences";
import { failToLoad, renderRoute } from "@/test/utils";
import type { SequenceSummary, SequencesSummary } from "@/types/report";

const SEQUENCE: SequenceSummary = {
  name: "actor_actor_id_seq",
  sequenceUrl: "#/sequences/actor_actor_id_seq-1a2b3c4d",
  type: "bigint",
  start: 1,
  increment: 1,
  minValue: 1,
  maxValue: 2147483647,
  cache: "20",
  cycle: false,
  isOrdered: true,
};

function renderSequences(sequences?: SequencesSummary) {
  return renderRoute({
    path: "/sequences",
    component: SequencesPage,
    data: { summaries: sequences === undefined ? {} : { sequences } },
  });
}

describe("SequencesPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderSequences();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderSequences();
    expect(await screen.findByText("Failed to load sequences: boom")).toBeInTheDocument();
  });

  it("links each row's name to its sequence detail route, derived from the hash url", async () => {
    await renderSequences({ sequencesCount: 1, allSequences: [SEQUENCE] });
    expect(screen.getByRole("link", { name: "actor_actor_id_seq" })).toHaveAttribute(
      "href",
      "/sequences/actor_actor_id_seq-1a2b3c4d",
    );
  });

  it("distinguishes a cycling sequence from one that stops at its bound", async () => {
    await renderSequences({
      sequencesCount: 2,
      allSequences: [
        SEQUENCE,
        { ...SEQUENCE, name: "ticket_seq", sequenceUrl: "#/sequences/ticket-5e6f", cycle: true },
      ],
    });
    expect(screen.getByLabelText("Cycles")).toBeInTheDocument();
    expect(screen.getByLabelText("Does not cycle")).toBeInTheDocument();
  });

  it("shows an em dash for an absent bound and an unreported cache", async () => {
    await renderSequences({
      sequencesCount: 1,
      allSequences: [{ ...SEQUENCE, minValue: undefined, maxValue: undefined, cache: "" }],
    });
    expect(screen.getAllByText("—")).toHaveLength(3);
  });

  it("shows the sequences count in the heading", async () => {
    await renderSequences({ sequencesCount: 5, allSequences: [] });
    expect(screen.getByText("(5)")).toBeInTheDocument();
    expect(screen.getByText("No sequences.")).toBeInTheDocument();
  });
});
