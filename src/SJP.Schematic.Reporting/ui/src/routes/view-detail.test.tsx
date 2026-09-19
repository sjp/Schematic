import { render, screen, within } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useDetail } from "@/hooks/useReportData";
import { ViewDetailPage } from "@/routes/view-detail";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { ViewDetail } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useDetail: vi.fn<typeof useDetail>(),
}));

// The route reads its param through `getRouteApi` and links with `Link`; stub both so the page can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({ useParams: () => ({ viewKey: "staff_list-1a2b" }) }),
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

const mockUseDetail = vi.mocked(useDetail<ViewDetail>);

const VIEW: ViewDetail = {
  name: "staff_list",
  viewUrl: "#/views/staff_list-1a2b",
  definition: "SELECT s.staff_id FROM staff s",
  columns: [
    {
      ordinal: 1,
      columnName: "staff_id",
      isNullable: false,
      type: "integer",
      defaultValue: "",
    },
    {
      ordinal: 2,
      columnName: "zip_code",
      isNullable: true,
      type: "postcode",
      typeUrl: "#/user-defined-types/postcode-9f8e",
      defaultValue: "'0000'",
    },
  ],
  columnsCount: 2,
  referencedObjects: [],
  referencedObjectsCount: 0,
  indexes: [],
  indexesCount: 0,
  triggers: [],
  triggersCount: 0,
  checkOption: "",
  isUpdatable: false,
  isMaterialized: false,
  refreshMode: "",
  refreshMethod: "",
  isPopulated: true,
};

describe("ViewDetailPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseDetail.mockReturnValue(pendingQuery());

    render(<ViewDetailPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseDetail.mockReturnValue(failedQuery(new Error("boom")));

    render(<ViewDetailPage />);
    expect(screen.getByText("Failed to load view: boom")).toBeInTheDocument();
  });

  it("heads the page with the view's name and column count, under a link back to the list", () => {
    mockUseDetail.mockReturnValue(loadedQuery(VIEW));

    render(<ViewDetailPage />);
    expect(screen.getByRole("heading", { name: "staff_list" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Views" })).toHaveAttribute("href", "/views");
    expect(screen.getByText("2 columns")).toBeInTheDocument();
  });

  it("asks for the lint findings raised against this view", () => {
    mockUseDetail.mockReturnValue(loadedQuery(VIEW));

    render(<ViewDetailPage />);
    expect(screen.getByTestId("lint")).toHaveAttribute(
      "data-object-url",
      "#/views/staff_list-1a2b",
    );
  });

  it("lists the columns, linking a user-defined type to its own page", () => {
    mockUseDetail.mockReturnValue(loadedQuery(VIEW));

    render(<ViewDetailPage />);
    expect(screen.getByText("staff_id")).toBeInTheDocument();
    expect(screen.getByText("integer")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "postcode" })).toHaveAttribute(
      "href",
      "#/user-defined-types/postcode-9f8e",
    );
    expect(screen.getByLabelText("Nullable")).toBeInTheDocument();
    expect(screen.getByLabelText("Not nullable")).toBeInTheDocument();
    expect(screen.getByText("'0000'")).toBeInTheDocument();
  });

  it("describes a materialized view's refresh settings", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...VIEW,
        isMaterialized: true,
        refreshMode: "ON DEMAND",
        refreshMethod: "FAST",
        isPopulated: false,
      }),
    );

    render(<ViewDetailPage />);
    expect(
      screen.getByText("Materialized · refreshed ON DEMAND · FAST · not populated"),
    ).toBeInTheDocument();
  });

  it("marks an updatable view with its check option", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({ ...VIEW, isUpdatable: true, checkOption: "WITH CASCADED CHECK OPTION" }),
    );

    render(<ViewDetailPage />);
    expect(screen.getByText("Updatable · WITH CASCADED CHECK OPTION")).toBeInTheDocument();
  });

  it("leaves out the optional sections a plain view has nothing to fill them with", () => {
    mockUseDetail.mockReturnValue(loadedQuery(VIEW));

    render(<ViewDetailPage />);
    expect(screen.queryByRole("heading", { name: /Referenced Objects/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Indexes/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Triggers/u })).not.toBeInTheDocument();
  });

  it("links the objects the definition references", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...VIEW,
        referencedObjects: [{ name: "staff", url: "#/tables/staff-7a8b" }],
        referencedObjectsCount: 1,
      }),
    );

    render(<ViewDetailPage />);
    expect(screen.getByRole("link", { name: "staff" })).toHaveAttribute(
      "href",
      "#/tables/staff-7a8b",
    );
  });

  it("lists a materialized view's indexes with their usability", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...VIEW,
        indexes: [
          {
            name: "staff_list_pk",
            isUnique: true,
            columnsText: "staff_id",
            includedColumnsText: "",
            indexType: "",
            filterText: "",
            isEnabled: true,
            isValid: false,
            isVisible: true,
          },
        ],
        indexesCount: 1,
      }),
    );

    render(<ViewDetailPage />);
    expect(screen.getByRole("heading", { name: "Indexes(1)" })).toBeInTheDocument();
    expect(screen.getByText("staff_list_pk")).toBeInTheDocument();
    expect(screen.getByLabelText("Unique index")).toBeInTheDocument();
    expect(screen.getByText("Invalid")).toBeInTheDocument();
    // Included columns, index type and filter are all unreported here.
    expect(screen.getAllByText("—")).toHaveLength(3);
  });

  it("shows an unnamed, non-unique index as an em dash", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...VIEW,
        indexes: [
          {
            name: "",
            isUnique: false,
            columnsText: "staff_id",
            includedColumnsText: "last_name",
            indexType: "B-Tree",
            filterText: "staff_id > 0",
            isEnabled: true,
            isValid: true,
            isVisible: true,
          },
        ],
        indexesCount: 1,
      }),
    );

    render(<ViewDetailPage />);
    expect(screen.getByLabelText("Non-unique index")).toBeInTheDocument();
    expect(screen.getByText("Usable")).toBeInTheDocument();
    expect(screen.getAllByText("—")).toHaveLength(1);
  });

  it("shows each trigger's timing, columns, condition and body", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...VIEW,
        triggers: [
          {
            triggerName: "staff_list_ins",
            definition: "BEGIN INSERT INTO staff ... END",
            queryTiming: "INSTEAD OF",
            events: "INSERT",
            granularity: "FOR EACH ROW",
            condition: "NEW.staff_id > 0",
            updateColumns: "last_name",
          },
        ],
        triggersCount: 1,
      }),
    );

    render(<ViewDetailPage />);
    expect(screen.getByText("staff_list_ins")).toBeInTheDocument();
    expect(screen.getByText("INSTEAD OF INSERT OF last_name FOR EACH ROW")).toBeInTheDocument();
    expect(screen.getByText("NEW.staff_id > 0")).toBeInTheDocument();
    expect(screen.getByText("BEGIN INSERT INTO staff ... END")).toBeInTheDocument();
  });

  it("always shows the definition", () => {
    mockUseDetail.mockReturnValue(loadedQuery(VIEW));

    render(<ViewDetailPage />);
    const definition = screen.getByRole("heading", { name: "Definition" }).parentElement;
    expect(definition).not.toBeNull();
    expect(within(definition!).getByText("SELECT s.staff_id FROM staff s")).toBeInTheDocument();
  });
});
