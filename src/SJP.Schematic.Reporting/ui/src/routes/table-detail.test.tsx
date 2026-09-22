import { fireEvent, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";

import { useDetail, useSummary } from "@/hooks/useReportData";
import { TableDetailPage } from "@/routes/table-detail";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type {
  GraphTable,
  KeyConstraint,
  RelationshipGraph,
  RelationshipsSummary,
  TableColumn,
  TableDetail,
} from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useDetail: vi.fn<typeof useDetail>(),
  useSummary: vi.fn<typeof useSummary>(),
}));

// The route reads its param through `getRouteApi` and links with `Link`; stub both so the page can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({ useParams: () => ({ tableKey: "film_actor" }) }),
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
  LintFindings: () => null,
}));

// ELK layout does not run under a headless DOM; list the tables the diagram was given instead.
vi.mock("@/components/RelationshipDiagram", () => ({
  RelationshipDiagram: ({ graph }: { graph: RelationshipGraph }) => (
    <ul aria-label="diagram">
      {graph.nodes.map((n) => (
        <li key={n.id}>
          {n.id}
          {n.isHighlighted === true ? " (focal)" : ""}
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

/** An ordinary column, with nothing about it worth marking up. Tests override what they exercise. */
function column(overrides: Partial<TableColumn>): TableColumn {
  return {
    ordinal: 1,
    columnName: "actor_id",
    isNullable: false,
    type: "integer",
    defaultValue: "",
    isPrimaryKey: false,
    isUniqueKey: false,
    isForeignKey: false,
    parentKeys: [],
    parentKeysCount: 0,
    childKeys: [],
    childKeysCount: 0,
    isAutoIncrement: false,
    identityGeneration: "",
    identitySequenceName: "",
    isComputed: false,
    computedDefinition: "",
    computedStorage: "",
    isHidden: false,
    ...overrides,
  };
}

/** A validated, non-deferrable key constraint — the ordinary case. */
const KEY: KeyConstraint = {
  constraintName: "film_actor_pkey",
  columnNames: "actor_id",
  isValidated: true,
  deferrabilityDescription: "",
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

function stubData({
  table = TABLE,
  relationships,
}: {
  table?: TableDetail;
  relationships?: RelationshipsSummary;
}) {
  mockUseDetail.mockReturnValue(loadedQuery(table));
  mockUseSummary.mockImplementation(() =>
    relationships === undefined ? pendingQuery() : loadedQuery(relationships),
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

  it("shows a loading indicator while the table itself is pending", () => {
    mockUseDetail.mockReturnValue(pendingQuery());
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<TableDetailPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message when the table fails to load", () => {
    mockUseDetail.mockReturnValue(failedQuery(new Error("boom")));
    mockUseSummary.mockReturnValue(pendingQuery());

    render(<TableDetailPage />);
    expect(screen.getByText("Failed to load table: boom")).toBeInTheDocument();
  });

  it("keeps the rest of the page when only the relationships data fails", () => {
    mockUseDetail.mockReturnValue(loadedQuery(TABLE));
    mockUseSummary.mockReturnValue(failedQuery(new Error("no graph")));

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "film_actor" })).toBeInTheDocument();
    expect(screen.getByText("Failed to load relationships: no graph")).toBeInTheDocument();
  });

  it("heads the page with the table's name and column count, under a link back to the list", () => {
    stubData({ table: { ...TABLE, columnsCount: 3 } });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "film_actor" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Tables" })).toHaveAttribute("href", "/tables");
    expect(screen.getByText("3 columns")).toBeInTheDocument();
  });

  it("badges the storage attributes that set a table apart from an ordinary one", () => {
    stubData({
      table: { ...TABLE, kind: "History", isLogged: false, collation: "en_US.utf8" },
    });

    render(<TableDetailPage />);
    expect(screen.getByText("History")).toBeInTheDocument();
    expect(screen.getByText("Unlogged")).toBeInTheDocument();
    expect(screen.getByText("Collation · en_US.utf8")).toBeInTheDocument();
  });

  it("leaves the storage badges off an ordinary logged table", () => {
    stubData({});

    render(<TableDetailPage />);
    expect(screen.queryByText("Unlogged")).not.toBeInTheDocument();
    expect(screen.queryByText(/^Collation/u)).not.toBeInTheDocument();
  });

  it("lists the columns, linking a user-defined type to its own page", () => {
    stubData({
      table: {
        ...TABLE,
        columns: [
          column({ ordinal: 1, columnName: "actor_id", type: "integer" }),
          column({
            ordinal: 2,
            columnName: "zip",
            type: "postcode",
            typeUrl: "#/user-defined-types/postcode-9f8e",
            isNullable: true,
            defaultValue: "'0000'",
          }),
        ],
        columnsCount: 2,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByText("actor_id")).toBeInTheDocument();
    expect(screen.getByText("integer")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "postcode" })).toHaveAttribute(
      "href",
      "#/user-defined-types/postcode-9f8e",
    );
    expect(screen.getByLabelText("Nullable")).toBeInTheDocument();
    expect(screen.getByLabelText("Not nullable")).toBeInTheDocument();
    expect(screen.getByText("'0000'")).toBeInTheDocument();
  });

  it("marks up everything notable about a column with its own icon", () => {
    stubData({
      table: {
        ...TABLE,
        columns: [
          column({
            columnName: "actor_id",
            isPrimaryKey: true,
            isUniqueKey: true,
            isForeignKey: true,
            isAutoIncrement: true,
            isComputed: true,
            isHidden: true,
            parentKeys: [
              {
                constraintDescription: "film_actor_actor_id_fkey",
                parentTableName: "actor",
                parentTableUrl: "#/tables/actor-d4592e62",
                parentColumnName: "actor_id",
              },
            ],
            parentKeysCount: 1,
          }),
        ],
        columnsCount: 1,
      },
    });

    render(<TableDetailPage />);
    for (const label of [
      "Primary key",
      "Unique key",
      "Foreign key",
      "Generated value",
      "Computed value",
      "Hidden column",
    ]) {
      expect(screen.getByLabelText(label)).toBeInTheDocument();
    }
  });

  it("leaves an ordinary column unmarked", () => {
    stubData({ table: { ...TABLE, columns: [column({})], columnsCount: 1 } });

    render(<TableDetailPage />);
    expect(screen.queryByLabelText("Primary key")).not.toBeInTheDocument();
    expect(screen.queryByLabelText("Hidden column")).not.toBeInTheDocument();
  });

  it("names the constraint behind a key icon in its tooltip", async () => {
    const user = userEvent.setup();
    stubData({
      table: {
        ...TABLE,
        columns: [column({ columnName: "actor_id", isUniqueKey: true })],
        columnsCount: 1,
        // Only the constraint covering `actor_id` is named; the other column's is not.
        uniqueKeys: [
          { ...KEY, constraintName: "film_actor_actor_id_key", columnNames: "actor_id, film_id" },
          { ...KEY, constraintName: "film_actor_film_id_key", columnNames: "film_id" },
        ],
        uniqueKeysCount: 2,
      },
    });

    render(<TableDetailPage />);
    // Radix opens the tooltip on hover and closes it again on click.
    await user.hover(screen.getByLabelText("Unique key"));

    const tooltip = await screen.findByRole("tooltip");
    expect(tooltip).toHaveTextContent("Unique key · film_actor_actor_id_key");
    expect(tooltip).not.toHaveTextContent("film_actor_film_id_key");
  });

  it("shows a table's system versioning", () => {
    stubData({
      table: {
        ...TABLE,
        tableSystemVersioning: {
          historyTable: {
            name: "film_actor_history",
            tableUrl: "#/tables/film_actor_history-7a8b",
          },
          periodStartColumn: "valid_from",
          periodEndColumn: "valid_to",
        },
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "System Versioning" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "film_actor_history" })).toHaveAttribute(
      "href",
      "#/tables/film_actor_history-7a8b",
    );
    expect(screen.getByText("valid_from")).toBeInTheDocument();
    expect(screen.getByText("valid_to")).toBeInTheDocument();
  });

  it("names a history table the report has no page for without linking it", () => {
    stubData({
      table: {
        ...TABLE,
        tableSystemVersioning: {
          historyTable: { name: "film_actor_history", tableUrl: "" },
          periodStartColumn: "valid_from",
          periodEndColumn: "valid_to",
        },
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByText("film_actor_history")).toBeInTheDocument();
    expect(screen.queryByRole("link", { name: "film_actor_history" })).not.toBeInTheDocument();
  });

  it("shows a table's partitioning, linking each partition", () => {
    stubData({
      table: {
        ...TABLE,
        tablePartitioning: {
          strategy: "RANGE",
          columnNames: ["last_update"],
          partitions: [
            { name: "film_actor_2024", tableUrl: "#/tables/film_actor_2024-1a2b" },
            { name: "film_actor_2025", tableUrl: "#/tables/film_actor_2025-3c4d" },
          ],
          partitionsCount: 2,
        },
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "Partitioning(2)" })).toBeInTheDocument();
    expect(screen.getByText("RANGE")).toBeInTheDocument();
    expect(screen.getByText("last_update")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "film_actor_2024" })).toHaveAttribute(
      "href",
      "#/tables/film_actor_2024-1a2b",
    );
  });

  it("shows an em dash for unreported partition key columns and an unlisted partition set", () => {
    stubData({
      table: {
        ...TABLE,
        tablePartitioning: {
          strategy: "HASH",
          columnNames: [],
          partitions: [],
          partitionsCount: 0,
        },
      },
    });

    render(<TableDetailPage />);
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("shows the primary key with its status", () => {
    stubData({
      table: {
        ...TABLE,
        primaryKeyExists: true,
        primaryKey: { ...KEY, constraintName: "film_actor_pkey", columnNames: "actor_id, film_id" },
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "Primary Key" })).toBeInTheDocument();
    expect(screen.getByText("film_actor_pkey")).toBeInTheDocument();
    expect(screen.getByText("actor_id, film_id")).toBeInTheDocument();
    expect(screen.getByText("Enforced")).toBeInTheDocument();
  });

  it("shows an em dash for an unnamed key constraint", () => {
    stubData({
      table: {
        ...TABLE,
        primaryKeyExists: true,
        primaryKey: { ...KEY, constraintName: "" },
        uniqueKeys: [{ ...KEY, constraintName: "" }],
        uniqueKeysCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("lists the unique keys with their status", () => {
    stubData({
      table: {
        ...TABLE,
        uniqueKeys: [{ ...KEY, constraintName: "film_actor_film_id_key", isValidated: false }],
        uniqueKeysCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "Unique Keys(1)" })).toBeInTheDocument();
    expect(screen.getByText("film_actor_film_id_key")).toBeInTheDocument();
    expect(screen.getByText("Not validated")).toBeInTheDocument();
  });

  it("lists the foreign keys, linking each parent table", () => {
    stubData({
      table: {
        ...TABLE,
        foreignKeys: [
          {
            constraintName: "film_actor_actor_id_fkey",
            parentConstraintName: "actor_pkey",
            childColumnNames: "actor_id",
            parentTableName: "actor",
            parentTableUrl: "#/tables/actor-d4592e62",
            parentColumnNames: "actor_id",
            deleteActionDescription: "CASCADE",
            updateActionDescription: "NO ACTION",
            matchTypeDescription: "MATCH FULL",
            isValidated: true,
            deferrabilityDescription: "",
          },
        ],
        foreignKeysCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "Foreign Keys(1)" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
    expect(screen.getByText("CASCADE")).toBeInTheDocument();
    expect(screen.getByText("NO ACTION")).toBeInTheDocument();
    expect(screen.getByText("MATCH FULL")).toBeInTheDocument();
    expect(screen.getByText("Enforced")).toBeInTheDocument();
  });

  it("shows an em dash for an unnamed foreign key and the default match type", () => {
    stubData({
      table: {
        ...TABLE,
        foreignKeys: [
          {
            constraintName: "",
            parentConstraintName: "actor_pkey",
            childColumnNames: "actor_id",
            parentTableName: "actor",
            parentTableUrl: "#/tables/actor-d4592e62",
            parentColumnNames: "actor_id",
            deleteActionDescription: "NO ACTION",
            updateActionDescription: "NO ACTION",
            matchTypeDescription: "",
            isValidated: true,
            deferrabilityDescription: "",
          },
        ],
        foreignKeysCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getAllByText("—")).toHaveLength(2);
  });

  it("lists the check constraints with their definitions", () => {
    stubData({
      table: {
        ...TABLE,
        checkConstraints: [
          {
            constraintName: "film_actor_check",
            definition: "actor_id > 0",
            isValidated: true,
            deferrabilityDescription: "",
          },
        ],
        checkConstraintsCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "Check Constraints(1)" })).toBeInTheDocument();
    expect(screen.getByText("film_actor_check")).toBeInTheDocument();
    expect(screen.getByText("actor_id > 0")).toBeInTheDocument();
  });

  it("lists the indexes with their usability", () => {
    stubData({
      table: {
        ...TABLE,
        indexes: [
          {
            name: "idx_fk_film_id",
            isUnique: false,
            columnsText: "film_id",
            includedColumnsText: "",
            indexType: "",
            filterText: "",
            isEnabled: false,
            isValid: true,
            isVisible: true,
          },
        ],
        indexesCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "Indexes(1)" })).toBeInTheDocument();
    expect(screen.getByText("idx_fk_film_id")).toBeInTheDocument();
    expect(screen.getByLabelText("Non-unique index")).toBeInTheDocument();
    expect(screen.getByText("Disabled")).toBeInTheDocument();
    // Type, included columns and filter are all unreported here.
    expect(screen.getAllByText("—")).toHaveLength(3);
  });

  it("shows an unnamed unique index as an em dash", () => {
    stubData({
      table: {
        ...TABLE,
        indexes: [
          {
            name: "",
            isUnique: true,
            columnsText: "actor_id, film_id",
            includedColumnsText: "last_update",
            indexType: "B-Tree",
            filterText: "actor_id > 0",
            isEnabled: true,
            isValid: true,
            isVisible: true,
          },
        ],
        indexesCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByLabelText("Unique index")).toBeInTheDocument();
    expect(screen.getByText("Usable")).toBeInTheDocument();
    expect(screen.getAllByText("—")).toHaveLength(1);
  });

  it("shows each trigger's timing, columns, condition and body", () => {
    stubData({
      table: {
        ...TABLE,
        triggers: [
          {
            triggerName: "ins_film_actor",
            definition: "BEGIN INSERT INTO audit ... END",
            queryTiming: "AFTER",
            events: "UPDATE",
            granularity: "FOR EACH ROW",
            condition: "NEW.actor_id <> OLD.actor_id",
            updateColumns: "actor_id",
          },
        ],
        triggersCount: 1,
      },
    });

    render(<TableDetailPage />);
    expect(screen.getByRole("heading", { name: "Triggers(1)" })).toBeInTheDocument();
    expect(screen.getByText("ins_film_actor")).toBeInTheDocument();
    expect(screen.getByText("AFTER UPDATE OF actor_id FOR EACH ROW")).toBeInTheDocument();
    expect(screen.getByText("NEW.actor_id <> OLD.actor_id")).toBeInTheDocument();
    expect(screen.getByText("BEGIN INSERT INTO audit ... END")).toBeInTheDocument();
  });

  it("leaves out every section the table has nothing to fill it with", () => {
    stubData({ relationships: RELATIONSHIPS });

    render(<TableDetailPage />);
    for (const title of [
      /System Versioning/u,
      /Partitioning/u,
      /Primary Key/u,
      /Unique Keys/u,
      /Foreign Keys/u,
      /Check Constraints/u,
      /Indexes/u,
      /Triggers/u,
    ]) {
      expect(screen.queryByRole("heading", { name: title })).not.toBeInTheDocument();
    }
    // Columns and Relationships are always shown.
    expect(screen.getByRole("heading", { name: "Columns(0)" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Relationships" })).toBeInTheDocument();
  });
});
