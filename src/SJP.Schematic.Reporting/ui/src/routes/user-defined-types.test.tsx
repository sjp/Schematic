import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { UserDefinedTypesPage } from "@/routes/user-defined-types";
import { failToLoad, renderRoute } from "@/test/utils";
import type { UserDefinedTypesSummary } from "@/types/report";

const TYPE = {
  name: "public.mood",
  typeUrl: "#/user-defined-types/mood-1a2b3c4d",
  kind: "Enum",
  baseType: "",
  isNullable: true,
  attributesCount: 0,
  enumValuesCount: 3,
};

function renderTypes(userDefinedTypes?: UserDefinedTypesSummary) {
  return renderRoute({
    path: "/user-defined-types",
    component: UserDefinedTypesPage,
    data: { summaries: userDefinedTypes === undefined ? {} : { userDefinedTypes } },
  });
}

describe("UserDefinedTypesPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderTypes();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderTypes();
    expect(await screen.findByText("Failed to load user-defined types: boom")).toBeInTheDocument();
  });

  it("links each row's name to its type detail route, derived from the hash url", async () => {
    await renderTypes({ userDefinedTypesCount: 1, allUserDefinedTypes: [TYPE] });
    const link = screen.getByRole("link", { name: "public.mood" });
    expect(link).toHaveAttribute("href", "/user-defined-types/mood-1a2b3c4d");
  });

  it("shows an em dash for a type that is not defined in terms of another", async () => {
    await renderTypes({ userDefinedTypesCount: 1, allUserDefinedTypes: [TYPE] });
    expect(screen.getByText("Enum")).toBeInTheDocument();
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("shows the types count in the heading", async () => {
    await renderTypes({ userDefinedTypesCount: 3, allUserDefinedTypes: [] });
    expect(screen.getByText("(3)")).toBeInTheDocument();
  });
});
