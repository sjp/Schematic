import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ViewsPage } from "@/routes/views";
import { failToLoad, renderRoute } from "@/test/utils";
import type { ViewsSummary } from "@/types/report";

const VIEW = {
  name: "staff_list",
  viewUrl: "#/views/staff_list-1a2b3c4d",
  columnCount: 8,
  isMaterialized: false,
};

function renderViews(views?: ViewsSummary) {
  return renderRoute({
    path: "/views",
    component: ViewsPage,
    data: { summaries: views === undefined ? {} : { views } },
  });
}

describe("ViewsPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderViews();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderViews();
    expect(await screen.findByText("Failed to load views: boom")).toBeInTheDocument();
  });

  it("links each row's name to its view detail route, derived from the hash url", async () => {
    await renderViews({ viewsCount: 1, allViews: [VIEW] });
    expect(screen.getByRole("link", { name: "staff_list" })).toHaveAttribute(
      "href",
      "/views/staff_list-1a2b3c4d",
    );
  });

  it("distinguishes materialized views from ordinary ones", async () => {
    await renderViews({
      viewsCount: 2,
      allViews: [
        VIEW,
        { ...VIEW, name: "sales_by_store", viewUrl: "#/views/sales-5e6f", isMaterialized: true },
      ],
    });
    expect(screen.getByLabelText("Materialized")).toBeInTheDocument();
    expect(screen.getByLabelText("Not materialized")).toBeInTheDocument();
  });

  it("shows the views count in the heading", async () => {
    await renderViews({ viewsCount: 7, allViews: [] });
    expect(screen.getByText("(7)")).toBeInTheDocument();
    expect(screen.getByText("No views.")).toBeInTheDocument();
  });
});
