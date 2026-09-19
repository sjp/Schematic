import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { SequencesPage } from "@/routes/sequences";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { SequenceSummary, SequencesSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

// `sequences.tsx` only imports `Link` from this package; stub it as a plain anchor so the route can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  Link: ({
    to,
    params,
    children,
    className,
  }: {
    to: string;
    params: Record<string, string>;
    children: React.ReactNode;
    className?: string;
  }) => {
    const href = Object.entries(params).reduce(
      (path, [key, value]) => path.replace(`$${key}`, value),
      to,
    );
    return (
      <a href={href} className={className}>
        {children}
      </a>
    );
  },
}));

const mockUseSummary = vi.mocked(useSummary<SequencesSummary>);

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

describe("SequencesPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<SequencesPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("boom")));

    render(<SequencesPage />);
    expect(screen.getByText("Failed to load sequences: boom")).toBeInTheDocument();
  });

  it("links each row's name to its sequence detail route, derived from the hash url", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ sequencesCount: 1, allSequences: [SEQUENCE] }));

    render(<SequencesPage />);
    expect(screen.getByRole("link", { name: "actor_actor_id_seq" })).toHaveAttribute(
      "href",
      "/sequences/actor_actor_id_seq-1a2b3c4d",
    );
  });

  it("distinguishes a cycling sequence from one that stops at its bound", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        sequencesCount: 2,
        allSequences: [
          SEQUENCE,
          { ...SEQUENCE, name: "ticket_seq", sequenceUrl: "#/sequences/ticket-5e6f", cycle: true },
        ],
      }),
    );

    render(<SequencesPage />);
    expect(screen.getByLabelText("Cycles")).toBeInTheDocument();
    expect(screen.getByLabelText("Does not cycle")).toBeInTheDocument();
  });

  it("shows an em dash for an absent bound and an unreported cache", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        sequencesCount: 1,
        allSequences: [{ ...SEQUENCE, minValue: undefined, maxValue: undefined, cache: "" }],
      }),
    );

    render(<SequencesPage />);
    expect(screen.getAllByText("—")).toHaveLength(3);
  });

  it("shows the sequences count in the heading", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ sequencesCount: 5, allSequences: [] }));

    render(<SequencesPage />);
    expect(screen.getByText("(5)")).toBeInTheDocument();
    expect(screen.getByText("No sequences.")).toBeInTheDocument();
  });
});
