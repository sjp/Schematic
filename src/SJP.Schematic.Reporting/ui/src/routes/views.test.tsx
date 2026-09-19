import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { ViewsPage } from "@/routes/views";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { ViewsSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn<typeof useSummary>(),
}));

// `views.tsx` only imports `Link` from this package; stub it as a plain anchor so the route can
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

const mockUseSummary = vi.mocked(useSummary<ViewsSummary>);

const VIEW = {
  name: "staff_list",
  viewUrl: "#/views/staff_list-1a2b3c4d",
  columnCount: 8,
  isMaterialized: false,
};

describe("ViewsPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<ViewsPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue(failedQuery(new Error("boom")));

    render(<ViewsPage />);
    expect(screen.getByText("Failed to load views: boom")).toBeInTheDocument();
  });

  it("links each row's name to its view detail route, derived from the hash url", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ viewsCount: 1, allViews: [VIEW] }));

    render(<ViewsPage />);
    expect(screen.getByRole("link", { name: "staff_list" })).toHaveAttribute(
      "href",
      "/views/staff_list-1a2b3c4d",
    );
  });

  it("distinguishes materialized views from ordinary ones", () => {
    mockUseSummary.mockReturnValue(
      loadedQuery({
        viewsCount: 2,
        allViews: [
          VIEW,
          { ...VIEW, name: "sales_by_store", viewUrl: "#/views/sales-5e6f", isMaterialized: true },
        ],
      }),
    );

    render(<ViewsPage />);
    expect(screen.getByLabelText("Materialized")).toBeInTheDocument();
    expect(screen.getByLabelText("Not materialized")).toBeInTheDocument();
  });

  it("shows the views count in the heading", () => {
    mockUseSummary.mockReturnValue(loadedQuery({ viewsCount: 7, allViews: [] }));

    render(<ViewsPage />);
    expect(screen.getByText("(7)")).toBeInTheDocument();
    expect(screen.getByText("No views.")).toBeInTheDocument();
  });
});
