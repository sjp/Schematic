import { render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useDetail } from "@/hooks/useReportData";
import { RoutineDetailPage } from "@/routes/routine-detail";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { RoutineDetail, RoutineParameter } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useDetail: vi.fn<typeof useDetail>(),
}));

// The route reads its param through `getRouteApi` and links with `Link`; stub both so the page can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({ useParams: () => ({ routineKey: "film_in_stock-1a2b" }) }),
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

const mockUseDetail = vi.mocked(useDetail<RoutineDetail>);

const PARAMETER: RoutineParameter = {
  parameterName: "p_film_id",
  type: "integer",
  direction: "Input",
  defaultValue: "0",
  ordinal: 1,
};

const ROUTINE: RoutineDetail = {
  name: "film_in_stock",
  routineUrl: "#/routines/film_in_stock-1a2b",
  definition: "BEGIN SELECT 1; END",
  routineType: "Procedure",
  language: "plpgsql",
  parameters: [PARAMETER],
  parametersCount: 1,
  returnType: "integer",
  overloads: [],
  overloadsCount: 0,
  referencedObjects: [],
  referencedObjectsCount: 0,
};

describe("RoutineDetailPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseDetail.mockReturnValue(pendingQuery());

    render(<RoutineDetailPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseDetail.mockReturnValue(failedQuery(new Error("boom")));

    render(<RoutineDetailPage />);
    expect(screen.getByText("Failed to load routine: boom")).toBeInTheDocument();
  });

  it("heads the page with the routine's name, under a link back to the list", () => {
    mockUseDetail.mockReturnValue(loadedQuery(ROUTINE));

    render(<RoutineDetailPage />);
    expect(screen.getByRole("heading", { name: "film_in_stock" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Routines" })).toHaveAttribute("href", "/routines");
  });

  it("asks for the lint findings raised against this routine", () => {
    mockUseDetail.mockReturnValue(loadedQuery(ROUTINE));

    render(<RoutineDetailPage />);
    expect(screen.getByTestId("lint")).toHaveAttribute(
      "data-object-url",
      "#/routines/film_in_stock-1a2b",
    );
  });

  it("shows the kind, language and return type", () => {
    mockUseDetail.mockReturnValue(loadedQuery(ROUTINE));

    render(<RoutineDetailPage />);
    expect(screen.getByText("Procedure")).toBeInTheDocument();
    expect(screen.getByText("plpgsql")).toBeInTheDocument();
    // "integer" is both the return type and the parameter's type.
    expect(screen.getAllByText("integer")).toHaveLength(2);
  });

  it("shows an em dash for an unrecorded language and a routine that returns nothing", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({ ...ROUTINE, language: undefined, returnType: undefined }),
    );

    render(<RoutineDetailPage />);
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("links a return type the report has a page for", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...ROUTINE,
        returnType: "film_summary",
        returnTypeUrl: "#/user-defined-types/film_summary-9f8e",
      }),
    );

    render(<RoutineDetailPage />);
    expect(screen.getByRole("link", { name: "film_summary" })).toHaveAttribute(
      "href",
      "#/user-defined-types/film_summary-9f8e",
    );
  });

  it("lists the parameters with their direction and default", () => {
    mockUseDetail.mockReturnValue(loadedQuery(ROUTINE));

    render(<RoutineDetailPage />);
    expect(screen.getByRole("heading", { name: "Parameters(1)" })).toBeInTheDocument();
    expect(screen.getByText("p_film_id")).toBeInTheDocument();
    expect(screen.getByText("In")).toBeInTheDocument();
    expect(screen.getByText("0")).toBeInTheDocument();
  });

  it.each([
    ["Input", "In"],
    ["Output", "Out"],
    ["InputOutput", "In/Out"],
  ] as const)("spells the %s direction as %s", (direction, expected) => {
    mockUseDetail.mockReturnValue(
      loadedQuery({ ...ROUTINE, parameters: [{ ...PARAMETER, direction }] }),
    );

    render(<RoutineDetailPage />);
    expect(screen.getByText(expected)).toBeInTheDocument();
  });

  it("marks a positional parameter and leaves the default cell blank when it has none", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...ROUTINE,
        parameters: [{ ...PARAMETER, parameterName: undefined, defaultValue: undefined }],
      }),
    );

    render(<RoutineDetailPage />);
    expect(screen.getByText("positional")).toBeInTheDocument();
  });

  it("says so when the routine takes no parameters", () => {
    mockUseDetail.mockReturnValue(loadedQuery({ ...ROUTINE, parameters: [], parametersCount: 0 }));

    render(<RoutineDetailPage />);
    expect(screen.getByText("No parameters.")).toBeInTheDocument();
  });

  it("shows the definition of a routine carrying a single signature", () => {
    mockUseDetail.mockReturnValue(loadedQuery(ROUTINE));

    render(<RoutineDetailPage />);
    expect(screen.getByRole("heading", { name: "Definition" })).toBeInTheDocument();
    expect(screen.getByText("BEGIN SELECT 1; END")).toBeInTheDocument();
  });

  it("replaces the parameters and definition sections with per-overload ones", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...ROUTINE,
        overloads: [
          {
            definition: "BEGIN RETURN 1; END",
            parameters: [PARAMETER],
            returnType: "integer",
          },
          {
            definition: "BEGIN RETURN 'a'; END",
            parameters: [],
          },
        ],
        overloadsCount: 2,
      }),
    );

    render(<RoutineDetailPage />);
    expect(screen.getByRole("heading", { name: "Overloads(2)" })).toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /^Parameters/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: "Definition" })).not.toBeInTheDocument();
    expect(screen.getByText("BEGIN RETURN 1; END")).toBeInTheDocument();
    expect(screen.getByText("BEGIN RETURN 'a'; END")).toBeInTheDocument();
    // The second overload returns nothing, so only the first names a return type.
    expect(screen.getByRole("heading", { name: "Overload 1 → integer" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Overload 2" })).toBeInTheDocument();
    expect(screen.getByText("No parameters.")).toBeInTheDocument();
  });

  it("links the objects the routine references", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...ROUTINE,
        referencedObjects: [{ name: "inventory", url: "#/tables/inventory-7a8b" }],
        referencedObjectsCount: 1,
      }),
    );

    render(<RoutineDetailPage />);
    const section = screen.getByRole("heading", { name: "Referenced Objects(1)" }).parentElement;
    expect(section).not.toBeNull();
    expect(within(section!).getByRole("link", { name: "inventory" })).toHaveAttribute(
      "href",
      "#/tables/inventory-7a8b",
    );
  });

  it("leaves out the referenced-objects section when the report found none", () => {
    mockUseDetail.mockReturnValue(loadedQuery(ROUTINE));

    render(<RoutineDetailPage />);
    expect(screen.queryByRole("heading", { name: /Referenced Objects/u })).not.toBeInTheDocument();
  });
});
