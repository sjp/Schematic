import { fireEvent, render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useDetail, useSummary } from "@/hooks/useReportData";
import { TableDetailPage } from "@/routes/table-detail";
import type {
  GraphTable,
  RelationshipGraph,
  RelationshipsSummary,
  TableDetail,
} from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useDetail: vi.fn(),
  useSummary: vi.fn(),
}));

// The route reads its param through `getRouteApi` and links with `Link`; stub both so the page can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({ useParams: () => ({ tableKey: "film_actor" }) }),
  Link: ({ children, className }: { children: React.ReactNode; className?: string }) => (
    <a className={className}>{children}</a>
  ),
}));

vi.mock("@/components/LintFindings", () => ({
  LintFindings: () => null,
}));

// ELK layout does not run under jsdom; list the tables the diagram was given instead.
vi.mock("@/components/RelationshipDiagram", () => ({
  RelationshipDiagram: ({ graph }: { graph: RelationshipGraph }) => (
    <ul aria-label="diagram">
      {graph.nodes.map((n) => (
        <li key={n.id}>
          {n.id}
          {n.isHighlighted ? " (focal)" : ""}
        </li>
      ))}
    </ul>
  ),
}));

const mockUseDetail = vi.mocked(useDetail);
const mockUseSummary = vi.mocked(useSummary);

const TABLE: TableDetail = {
  name: "film_actor",
  tableUrl: "#/tables/film_actor",
  columns: [],
  columnsCount: 0,
  primaryKeyExists: false,
  uniqueKeys: [],
  uniqueKeysCount: 0,
  foreignKeys: [],
  foreignKeysCount: 0,
  checkConstraints: [],
  checkConstraintsCount: 0,
  indexes: [],
  indexesCount: 0,
  triggers: [],
  triggersCount: 0,
  kind: "",
  isLogged: true,
  collation: "",
};

function node(id: string): GraphTable {
  return {
    id,
    name: id,
    tableUrl: `#/tables/${id}`,
    columns: [],
    columnsCount: 0,
    parentKeysCount: 0,
    childKeysCount: 0,
  };
}

// language <- film <- film_actor -> actor
const RELATIONSHIPS: RelationshipsSummary = {
  graph: {
    nodes: ["actor", "film", "film_actor", "language"].map(node),
    nodesCount: 4,
    edges: [
      ["film_actor", "actor"],
      ["film_actor", "film"],
      ["film", "language"],
    ].map(([child, parent]) => ({
      id: `${child}->${parent}`,
      childTableId: child!,
      parentTableId: parent!,
      constraintName: "",
      childColumns: [],
      parentColumns: [],
    })),
    edgesCount: 3,
  },
};

function stubData({ relationships }: { relationships?: RelationshipsSummary }) {
  mockUseDetail.mockReturnValue({
    isPending: false,
    isError: false,
    data: TABLE,
    error: null,
  } as never);
  mockUseSummary.mockImplementation(
    () =>
      ({
        isPending: relationships === undefined,
        isError: false,
        data: relationships,
        error: null,
      }) as never,
  );
}

function diagramTables() {
  const diagram = screen.getByRole("list", { name: "diagram" });
  return Array.from(diagram.querySelectorAll("li"), (li) => li.textContent);
}

describe("TableDetailPage", () => {
  it("reads the relationship diagrams from the schema-wide relationships data", () => {
    stubData({ relationships: RELATIONSHIPS });

    render(<TableDetailPage />);

    expect(mockUseSummary).toHaveBeenCalledWith("relationships");
    expect(diagramTables()).toEqual(["film_actor (focal)", "actor", "film"]);
  });

  it("widens the diagram to tables two relationships away", () => {
    stubData({ relationships: RELATIONSHIPS });

    render(<TableDetailPage />);
    fireEvent.click(screen.getByRole("button", { name: "Two Degrees" }));

    expect(diagramTables()).toEqual(["film_actor (focal)", "actor", "film", "language"]);
  });

  it("shows the table while the relationships data is still loading", () => {
    stubData({});

    render(<TableDetailPage />);

    expect(screen.getByRole("heading", { name: "film_actor" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Relationships" })).toBeInTheDocument();
    expect(screen.queryByRole("list", { name: "diagram" })).not.toBeInTheDocument();
  });
});
