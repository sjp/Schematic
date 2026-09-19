import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useDetail } from "@/hooks/useReportData";
import { SynonymDetailPage } from "@/routes/synonym-detail";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { SynonymDetail } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useDetail: vi.fn<typeof useDetail>(),
}));

// The route reads its param through `getRouteApi` and links with `Link`; stub both so the page can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({ useParams: () => ({ synonymKey: "actor_alias-1a2b" }) }),
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

const mockUseDetail = vi.mocked(useDetail<SynonymDetail>);

const SYNONYM: SynonymDetail = {
  name: "actor_alias",
  synonymUrl: "#/synonyms/actor_alias-1a2b",
  targetName: "dbo.actor",
  targetUrl: "#/tables/actor-d4592e62",
};

describe("SynonymDetailPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseDetail.mockReturnValue(pendingQuery());

    render(<SynonymDetailPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseDetail.mockReturnValue(failedQuery(new Error("boom")));

    render(<SynonymDetailPage />);
    expect(screen.getByText("Failed to load synonym: boom")).toBeInTheDocument();
  });

  it("heads the page with the synonym's name, under a link back to the list", () => {
    mockUseDetail.mockReturnValue(loadedQuery(SYNONYM));

    render(<SynonymDetailPage />);
    expect(screen.getByRole("heading", { name: "actor_alias" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Synonyms" })).toHaveAttribute("href", "/synonyms");
  });

  it("asks for the lint findings raised against this synonym", () => {
    mockUseDetail.mockReturnValue(loadedQuery(SYNONYM));

    render(<SynonymDetailPage />);
    expect(screen.getByTestId("lint")).toHaveAttribute(
      "data-object-url",
      "#/synonyms/actor_alias-1a2b",
    );
  });

  it("links a target the report has a page for", () => {
    mockUseDetail.mockReturnValue(loadedQuery(SYNONYM));

    render(<SynonymDetailPage />);
    expect(screen.getByRole("link", { name: "dbo.actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
  });

  it("names an unknown target as plain text rather than a dead link", () => {
    mockUseDetail.mockReturnValue(loadedQuery({ ...SYNONYM, targetUrl: undefined }));

    render(<SynonymDetailPage />);
    expect(screen.getByText("dbo.actor")).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "dbo.actor" })).not.toBeInTheDocument();
  });
});
