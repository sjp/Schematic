import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { RoutinesPage } from "@/routes/routines";
import { failToLoad, renderRoute } from "@/test/utils";
import type { RoutinesSummary, RoutineSummary } from "@/types/report";

const ROUTINE: RoutineSummary = {
  name: "film_in_stock",
  routineUrl: "#/routines/film_in_stock-1a2b3c4d",
  routineType: "Procedure",
};

function renderRoutines(routines?: RoutinesSummary) {
  return renderRoute({
    path: "/routines",
    component: RoutinesPage,
    data: { summaries: routines === undefined ? {} : { routines } },
  });
}

describe("RoutinesPage", () => {
  it("shows a loading indicator while pending", async () => {
    await renderRoutines();
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", async () => {
    failToLoad(new Error("boom"));

    await renderRoutines();
    expect(await screen.findByText("Failed to load routines: boom")).toBeInTheDocument();
  });

  it("links each row's name to its routine detail route, derived from the hash url", async () => {
    await renderRoutines({ routinesCount: 1, allRoutines: [ROUTINE] });
    expect(screen.getByRole("link", { name: "film_in_stock" })).toHaveAttribute(
      "href",
      "/routines/film_in_stock-1a2b3c4d",
    );
  });

  it("shows a known kind but leaves the cell blank for an unrecorded one", async () => {
    await renderRoutines({
      routinesCount: 2,
      allRoutines: [
        ROUTINE,
        {
          name: "get_customer_balance",
          routineUrl: "#/routines/get_customer_balance-5e6f",
          routineType: "Unknown",
        },
      ],
    });
    expect(screen.getByText("Procedure")).toBeInTheDocument();
    expect(screen.queryByText("Unknown")).not.toBeInTheDocument();
  });

  it("shows the routines count in the heading", async () => {
    await renderRoutines({ routinesCount: 4, allRoutines: [] });
    expect(screen.getByText("(4)")).toBeInTheDocument();
    expect(screen.getByText("No routines.")).toBeInTheDocument();
  });
});
