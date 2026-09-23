import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { SchemasPage } from "@/routes/schemas";
import { failToLoad, renderRoute } from "@/test/utils";
import type { SchemasSummary } from "@/types/report";

const SCHEMA = {
  name: "public",
  schemaUrl: "#/schemas/public-1a2b3c4d",
  owner: "postgres",
  isDefault: true,
  isSystem: false,
  tablesCount: 4,
  viewsCount: 1,
  sequencesCount: 2,
  synonymsCount: 0,
  routinesCount: 3,
  userDefinedTypesCount: 1,
  objectCount: 11,
};

function renderSchemas(schemas?: SchemasSummary) {
  return renderRoute({
    path: "/schemas",
    component: SchemasPage,
    data: { summaries: schemas === undefined ? {} : { schemas } },
  });
}

describe("SchemasPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderSchemas();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderSchemas();
    expect(await screen.findByText("Failed to load schemas: boom")).toBeInTheDocument();
  });

  it("links each row's name to its schema detail route, derived from the hash url", async () => {
    await renderSchemas({ schemasCount: 1, allSchemas: [SCHEMA] });
    const link = screen.getByRole("link", { name: "public" });
    expect(link).toHaveAttribute("href", "/schemas/public-1a2b3c4d");
  });

  it("marks the default schema and shows an em dash for an unrecorded owner", async () => {
    await renderSchemas({
      schemasCount: 2,
      allSchemas: [
        SCHEMA,
        {
          ...SCHEMA,
          name: "sys",
          schemaUrl: "#/schemas/sys-5e6f",
          owner: "",
          isDefault: false,
          isSystem: true,
        },
      ],
    });
    expect(screen.getByText("default")).toBeInTheDocument();
    expect(screen.getByText("system")).toBeInTheDocument();
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("shows the schemas count in the heading", async () => {
    await renderSchemas({ schemasCount: 3, allSchemas: [] });
    expect(screen.getByText("(3)")).toBeInTheDocument();
  });
});
