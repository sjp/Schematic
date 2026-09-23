import { screen, within } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ViewDetailPage } from "@/routes/view-detail";
import { lintMessage, lintSummary } from "@/test/lint";
import { failToLoad, renderRoute } from "@/test/utils";
import type { LintSummary, ViewDetail } from "@/types/report";

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

function renderView(view?: ViewDetail, lint?: LintSummary) {
  return renderRoute({
    path: "/views/$viewKey",
    url: "/views/staff_list-1a2b",
    component: ViewDetailPage,
    data: {
      details: view === undefined ? {} : { view: { "staff_list-1a2b": view } },
      summaries: lint === undefined ? {} : { lint },
    },
  });
}

describe("ViewDetailPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderView();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderView();
    expect(await screen.findByText("Failed to load view: boom")).toBeInTheDocument();
  });

  it("heads the page with the view's name and column count, under a link back to the list", async () => {
    await renderView(VIEW);
    expect(screen.getByRole("heading", { name: "staff_list" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "Views" })).toHaveAttribute("href", "/views");
    expect(screen.getByText("2 columns")).toBeInTheDocument();
  });

  it("shows the lint findings raised against this view", async () => {
    await renderView(
      VIEW,
      lintSummary([
        lintMessage({
          message: "This view is too wide.",
          objectUrl: "#/views/staff_list-1a2b",
        }),
        lintMessage({ message: "Something else is wrong.", objectUrl: "#/tables/actor-1a2b" }),
      ]),
    );
    expect(screen.getByText("This view is too wide.")).toBeInTheDocument();
    expect(screen.queryByText("Something else is wrong.")).not.toBeInTheDocument();
  });

  it("lists the columns, linking a user-defined type to its own page", async () => {
    await renderView(VIEW);
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

  it("describes a materialized view's refresh settings", async () => {
    await renderView({
      ...VIEW,
      isMaterialized: true,
      refreshMode: "ON DEMAND",
      refreshMethod: "FAST",
      isPopulated: false,
    });
    expect(
      screen.getByText("Materialized · refreshed ON DEMAND · FAST · not populated"),
    ).toBeInTheDocument();
  });

  it("marks an updatable view with its check option", async () => {
    await renderView({ ...VIEW, isUpdatable: true, checkOption: "WITH CASCADED CHECK OPTION" });
    expect(screen.getByText("Updatable · WITH CASCADED CHECK OPTION")).toBeInTheDocument();
  });

  it("leaves out the optional sections a plain view has nothing to fill them with", async () => {
    await renderView(VIEW);
    expect(screen.queryByRole("heading", { name: /Referenced Objects/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Indexes/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Triggers/u })).not.toBeInTheDocument();
  });

  it("links the objects the definition references", async () => {
    await renderView({
      ...VIEW,
      referencedObjects: [{ name: "staff", url: "#/tables/staff-7a8b" }],
      referencedObjectsCount: 1,
    });
    expect(screen.getByRole("link", { name: "staff" })).toHaveAttribute(
      "href",
      "#/tables/staff-7a8b",
    );
  });

  it("lists a materialized view's indexes with their usability", async () => {
    await renderView({
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
    });
    expect(screen.getByRole("heading", { name: "Indexes(1)" })).toBeInTheDocument();
    expect(screen.getByText("staff_list_pk")).toBeInTheDocument();
    expect(screen.getByLabelText("Unique index")).toBeInTheDocument();
    expect(screen.getByText("Invalid")).toBeInTheDocument();
    // Included columns, index type and filter are all unreported here.
    expect(screen.getAllByText("—")).toHaveLength(3);
  });

  it("shows an unnamed, non-unique index as an em dash", async () => {
    await renderView({
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
    });
    expect(screen.getByLabelText("Non-unique index")).toBeInTheDocument();
    expect(screen.getByText("Usable")).toBeInTheDocument();
    expect(screen.getAllByText("—")).toHaveLength(1);
  });

  it("shows each trigger's timing, columns, condition and body", async () => {
    await renderView({
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
    });
    expect(screen.getByText("staff_list_ins")).toBeInTheDocument();
    expect(screen.getByText("INSTEAD OF INSERT OF last_name FOR EACH ROW")).toBeInTheDocument();
    expect(screen.getByText("NEW.staff_id > 0")).toBeInTheDocument();
    expect(screen.getByText("BEGIN INSERT INTO staff ... END")).toBeInTheDocument();
  });

  it("always shows the definition", async () => {
    await renderView(VIEW);
    const definition = screen.getByRole("heading", { name: "Definition" }).parentElement;
    expect(definition).not.toBeNull();
    expect(within(definition!).getByText("SELECT s.staff_id FROM staff s")).toBeInTheDocument();
  });
});
