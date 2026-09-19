import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { SynonymsPage } from "@/routes/synonyms";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { SynonymsSummary, SynonymSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

// `synonyms.tsx` only imports `Link` from this package; stub it as a plain anchor so the route can
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

const mockUseSummary = vi.mocked(useSummary<SynonymsSummary>);

const SYNONYM: SynonymSummary = {
  name: "actor_alias",
  synonymUrl: "#/synonyms/actor_alias-1a2b3c4d",
  targetName: "dbo.actor",
  targetUrl: "#/tables/actor-d4592e62",
};

describe("SynonymsPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<SynonymsPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("boom")));

    render(<SynonymsPage />);
    expect(screen.getByText("Failed to load synonyms: boom")).toBeInTheDocument();
  });

  it("links each row's name to its synonym detail route, derived from the hash url", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ synonymsCount: 1, allSynonyms: [SYNONYM] }));

    render(<SynonymsPage />);
    expect(screen.getByRole("link", { name: "actor_alias" })).toHaveAttribute(
      "href",
      "/synonyms/actor_alias-1a2b3c4d",
    );
  });

  it("links a target the report has a page for", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ synonymsCount: 1, allSynonyms: [SYNONYM] }));

    render(<SynonymsPage />);
    expect(screen.getByRole("link", { name: "dbo.actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
  });

  it("names an unknown target as plain text rather than a dead link", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        synonymsCount: 1,
        allSynonyms: [{ ...SYNONYM, targetName: "other_db.thing", targetUrl: undefined }],
      }),
    );

    render(<SynonymsPage />);
    expect(screen.getByText("other_db.thing")).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "other_db.thing" })).not.toBeInTheDocument();
  });

  it("shows the synonyms count in the heading", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ synonymsCount: 2, allSynonyms: [] }));

    render(<SynonymsPage />);
    expect(screen.getByText("(2)")).toBeInTheDocument();
    expect(screen.getByText("No synonyms.")).toBeInTheDocument();
  });
});
