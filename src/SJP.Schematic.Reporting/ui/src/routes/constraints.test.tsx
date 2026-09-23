import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ConstraintsPage } from "@/routes/constraints";
import { failToLoad, renderWithClient } from "@/test/utils";
import type {
  CheckConstraintRow,
  ConstraintsSummary,
  ForeignKeyRow,
  PrimaryKeyConstraintRow,
  UniqueKeyRow,
} from "@/types/report";

const PRIMARY_KEY: PrimaryKeyConstraintRow = {
  tableName: "actor",
  tableUrl: "#/tables/actor-d4592e62",
  constraintName: "actor_pkey",
  columnNames: "actor_id",
  isValidated: true,
  deferrabilityDescription: "",
};

const UNIQUE_KEY: UniqueKeyRow = {
  tableName: "address",
  tableUrl: "#/tables/address-1a2b",
  constraintName: "address_postcode_key",
  columnNames: "postal_code",
  isValidated: true,
  deferrabilityDescription: "",
};

const FOREIGN_KEY: ForeignKeyRow = {
  tableName: "film_actor",
  tableUrl: "#/tables/film_actor-3c4d",
  constraintName: "film_actor_actor_id_fkey",
  childColumnNames: "actor_id",
  parentConstraintName: "actor_pkey",
  parentTableName: "actor",
  parentTableUrl: "#/tables/actor-d4592e62",
  parentColumnNames: "actor_id",
  deleteActionDescription: "CASCADE",
  updateActionDescription: "NO ACTION",
  matchTypeDescription: "MATCH FULL",
  isValidated: false,
  deferrabilityDescription: "",
};

const CHECK: CheckConstraintRow = {
  tableName: "film",
  tableUrl: "#/tables/film-5e6f",
  constraintName: "film_release_year_check",
  definition: "release_year > 1900",
  isValidated: true,
  deferrabilityDescription: "",
};

const EMPTY: ConstraintsSummary = {
  primaryKeys: [],
  primaryKeysCount: 0,
  uniqueKeys: [],
  uniqueKeysCount: 0,
  foreignKeys: [],
  foreignKeysCount: 0,
  checkConstraints: [],
  checkConstraintsCount: 0,
};

function renderConstraintsPage(summary?: ConstraintsSummary) {
  return renderWithClient(<ConstraintsPage />, {
    data: { summaries: summary === undefined ? {} : { constraints: summary } },
  });
}

describe("ConstraintsPage", () => {
  it("shows a loading indicator while pending", () => {
    renderConstraintsPage();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    renderConstraintsPage();
    expect(await screen.findByText("Failed to load constraints: boom")).toBeInTheDocument();
  });

  it("gives each constraint kind its own counted section", () => {
    renderConstraintsPage({
      primaryKeys: [PRIMARY_KEY],
      primaryKeysCount: 1,
      uniqueKeys: [UNIQUE_KEY],
      uniqueKeysCount: 1,
      foreignKeys: [FOREIGN_KEY],
      foreignKeysCount: 1,
      checkConstraints: [CHECK],
      checkConstraintsCount: 1,
    });
    for (const title of ["Primary Keys", "Unique Keys", "Foreign Keys", "Check Constraints"]) {
      // The count sits in a sibling span with no whitespace between it and the title.
      expect(screen.getByRole("heading", { name: `${title}(1)` })).toBeInTheDocument();
    }
  });

  it("says so in each section when the database declares no constraints at all", () => {
    renderConstraintsPage(EMPTY);
    expect(screen.getByText("No primary keys.")).toBeInTheDocument();
    expect(screen.getByText("No unique keys.")).toBeInTheDocument();
    expect(screen.getByText("No foreign keys.")).toBeInTheDocument();
    expect(screen.getByText("No check constraints.")).toBeInTheDocument();
  });

  it("links a primary key's table to its own page", () => {
    renderConstraintsPage({ ...EMPTY, primaryKeys: [PRIMARY_KEY], primaryKeysCount: 1 });
    expect(screen.getByRole("link", { name: "actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
    expect(screen.getByText("actor_pkey")).toBeInTheDocument();
    expect(screen.getByText("actor_id")).toBeInTheDocument();
  });

  it("links both ends of a foreign key and shows its referential actions", () => {
    renderConstraintsPage({ ...EMPTY, foreignKeys: [FOREIGN_KEY], foreignKeysCount: 1 });
    expect(screen.getByRole("link", { name: "film_actor" })).toHaveAttribute(
      "href",
      "#/tables/film_actor-3c4d",
    );
    expect(screen.getByRole("link", { name: "actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
    expect(screen.getByText("CASCADE")).toBeInTheDocument();
    expect(screen.getByText("NO ACTION")).toBeInTheDocument();
    expect(screen.getByText("MATCH FULL")).toBeInTheDocument();
  });

  it("carries each row's validation state through to its status cell", () => {
    renderConstraintsPage({
      ...EMPTY,
      primaryKeys: [PRIMARY_KEY],
      primaryKeysCount: 1,
      foreignKeys: [FOREIGN_KEY],
      foreignKeysCount: 1,
    });
    expect(screen.getByText("Enforced")).toBeInTheDocument();
    expect(screen.getByText("Not validated")).toBeInTheDocument();
  });

  it("shows a check constraint's definition", () => {
    renderConstraintsPage({ ...EMPTY, checkConstraints: [CHECK], checkConstraintsCount: 1 });
    expect(screen.getByText("release_year > 1900")).toBeInTheDocument();
  });
});
