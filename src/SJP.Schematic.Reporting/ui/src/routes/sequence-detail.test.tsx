import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useDetail } from "@/hooks/useReportData";
import { SequenceDetailPage } from "@/routes/sequence-detail";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { SequenceDetail } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useDetail: vi.fn<typeof useDetail>(),
}));

// The route reads its param through `getRouteApi` and links with `Link`; stub both so the page can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({ useParams: () => ({ sequenceKey: "actor_id_seq-1a2b" }) }),
  Link: ({
    to,
    children,
    className,
  }: {
    to?: string;
    children: React.ReactNode;
    className?: string;
  }) => (
    <a href={to ?? "#"} className={className}>
      {children}
    </a>
  ),
}));

vi.mock("@/components/LintFindings", () => ({
  LintFindings: ({ objectUrl }: { objectUrl: string }) => (
    <div data-testid="lint" data-object-url={objectUrl} />
  ),
}));

const mockUseDetail = vi.mocked(useDetail<SequenceDetail>);

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

describe("SequenceDetailPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseDetail.mockReturnValue(pendingQuery());

    render(<SequenceDetailPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseDetail.mockReturnValue(failedQuery(new Error("boom")));

    render(<SequenceDetailPage />);
    expect(screen.getByText("Failed to load sequence: boom")).toBeInTheDocument();
  });

  it("heads the page with the sequence's name, under a link back to the list", () => {
    mockUseDetail.mockReturnValue(loadedQuery(SEQUENCE));

    render(<SequenceDetailPage />);
    expect(screen.getByRole("heading", { name: "actor_actor_id_seq" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Sequences" })).toHaveAttribute("href", "/sequences");
  });

  it("asks for the lint findings raised against this sequence", () => {
    mockUseDetail.mockReturnValue(loadedQuery(SEQUENCE));

    render(<SequenceDetailPage />);
    expect(screen.getByTestId("lint")).toHaveAttribute(
      "data-object-url",
      "#/sequences/actor_id_seq-1a2b",
    );
  });

  it("lists every generation property the sequence declares", () => {
    mockUseDetail.mockReturnValue(loadedQuery(SEQUENCE));

    render(<SequenceDetailPage />);
    expect(screen.getByText("bigint")).toBeInTheDocument();
    expect(screen.getByText("2")).toBeInTheDocument();
    expect(screen.getByText("1000")).toBeInTheDocument();
    expect(screen.getByText("20")).toBeInTheDocument();
    // Cycle: yes; Ordered: no.
    expect(screen.getByText("Yes")).toBeInTheDocument();
    expect(screen.getByText("No")).toBeInTheDocument();
  });

  it("shows an em dash for an absent bound and an unreported cache", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({ ...SEQUENCE, minValue: undefined, maxValue: undefined, cache: "" }),
    );

    render(<SequenceDetailPage />);
    expect(screen.getAllByText("—")).toHaveLength(3);
  });
});
