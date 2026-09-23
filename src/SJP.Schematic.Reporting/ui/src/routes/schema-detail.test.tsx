import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { SchemaDetailPage } from "@/routes/schema-detail";
import { failToLoad, renderRoute } from "@/test/utils";
import type { SchemaDetail } from "@/types/report";

/** A one-object list, for filling a group whose contents the test does not care about. */
const object = (name: string) => [{ name, url: `#/${name}` }];

const EMPTY_SCHEMA: SchemaDetail = {
  name: "public",
  schemaUrl: "#/schemas/public-1a2b",
  owner: "postgres",
  isDefault: true,
  isSystem: false,
  tables: [],
  tablesCount: 0,
  views: [],
  viewsCount: 0,
  sequences: [],
  sequencesCount: 0,
  synonyms: [],
  synonymsCount: 0,
  routines: [],
  routinesCount: 0,
  userDefinedTypes: [],
  userDefinedTypesCount: 0,
  objectCount: 0,
};

function renderSchema(schema?: SchemaDetail) {
  return renderRoute({
    path: "/schemas/$schemaKey",
    url: "/schemas/public-1a2b",
    component: SchemaDetailPage,
    data: {
      details: schema === undefined ? {} : { schema: { "public-1a2b": schema } },
    },
  });
}

describe("SchemaDetailPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderSchema();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderSchema();
    expect(await screen.findByText("Failed to load schema: boom")).toBeInTheDocument();
  });

  it("heads the page with the schema's name, owner and default marker", async () => {
    await renderSchema(EMPTY_SCHEMA);
    expect(screen.getByRole("heading", { name: "public" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Schemas" })).toHaveAttribute("href", "/schemas");
    expect(screen.getByText("owned by postgres")).toBeInTheDocument();
    expect(screen.getByText("default")).toBeInTheDocument();
    expect(screen.queryByText("system")).not.toBeInTheDocument();
  });

  it("marks a system schema and omits the owner the database does not record", async () => {
    await renderSchema({ ...EMPTY_SCHEMA, owner: "", isDefault: false, isSystem: true });
    expect(screen.getByText("system")).toBeInTheDocument();
    expect(screen.queryByText(/owned by/u)).not.toBeInTheDocument();
    expect(screen.queryByText("default")).not.toBeInTheDocument();
  });

  it.each([
    [0, "0 objects"],
    [1, "1 object"],
    [2, "2 objects"],
  ])("pluralises the object count for %i", async (objectCount, expected) => {
    await renderSchema({ ...EMPTY_SCHEMA, objectCount });
    expect(screen.getByText(expected)).toBeInTheDocument();
  });

  it("says so rather than rendering empty sections when the schema holds nothing", async () => {
    await renderSchema(EMPTY_SCHEMA);
    expect(screen.getByText("This schema holds no objects the report covers.")).toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Tables/u })).not.toBeInTheDocument();
  });

  it("shows a counted, linked section for each kind of object the schema does hold", async () => {
    await renderSchema({
      ...EMPTY_SCHEMA,
      tables: [
        { name: "actor", url: "#/tables/actor-d4592e62" },
        { name: "film", url: "#/tables/film-5e6f" },
      ],
      tablesCount: 2,
      views: [{ name: "staff_list", url: "#/views/staff_list-3c4d" }],
      viewsCount: 1,
      objectCount: 3,
    });
    // The count sits in a sibling span with no whitespace between it and the label.
    expect(screen.getByRole("heading", { name: "Tables(2)" })).toBeInTheDocument();
    expect(screen.getByRole("heading", { name: "Views(1)" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "actor" })).toHaveAttribute(
      "href",
      "#/tables/actor-d4592e62",
    );
    expect(screen.getByRole("link", { name: "staff_list" })).toHaveAttribute(
      "href",
      "#/views/staff_list-3c4d",
    );
    // The four kinds the schema declares nothing for are left out entirely.
    expect(screen.queryByRole("heading", { name: /Sequences/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Routines/u })).not.toBeInTheDocument();
  });

  it("covers every kind of object a schema can hold", async () => {
    await renderSchema({
      ...EMPTY_SCHEMA,
      tables: object("t"),
      views: object("v"),
      sequences: object("s"),
      synonyms: object("sy"),
      routines: object("r"),
      userDefinedTypes: object("u"),
      objectCount: 6,
    });
    for (const label of [
      "Tables",
      "Views",
      "Sequences",
      "Synonyms",
      "Routines",
      "User-Defined Types",
    ]) {
      expect(screen.getByRole("heading", { name: `${label}(1)` })).toBeInTheDocument();
    }
  });
});
