import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { useSummary } from "@/hooks/useReportData";
import { TablesPage } from "@/routes/tables";
import type { TablesSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn(),
}));

// `tables.tsx` only imports `Link` from this package; stub it as a plain anchor
// so the route can render without a real TanStack Router context.
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

const mockUseSummary = vi.mocked(useSummary<TablesSummary>);

describe("TablesPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue({
      isPending: true,
      isError: false,
      data: undefined,
      error: null,
    } as never);

    render(<TablesPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: true,
      data: undefined,
      error: new Error("boom"),
    } as never);

    render(<TablesPage />);
    expect(screen.getByText("Failed to load tables: boom")).toBeInTheDocument();
  });

  it("links each row's name to its table detail route, derived from the hash url", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: {
        tablesCount: 1,
        allTables: [
          {
            name: "actor",
            tableUrl: "#/tables/actor-d4592e62",
            parentsCount: 0,
            childrenCount: 2,
            columnCount: 4,
            rowCount: 200,
          },
        ],
      },
      error: null,
    } as never);

    render(<TablesPage />);
    const link = screen.getByRole("link", { name: "actor" });
    expect(link).toHaveAttribute("href", "/tables/actor-d4592e62");
    // Large numbers are formatted with a thousands separator.
    expect(screen.getByText("200")).toBeInTheDocument();
  });

  it("shows the tables count in the heading", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: { tablesCount: 3, allTables: [] },
      error: null,
    } as never);

    render(<TablesPage />);
    expect(screen.getByText("(3)")).toBeInTheDocument();
  });
});
