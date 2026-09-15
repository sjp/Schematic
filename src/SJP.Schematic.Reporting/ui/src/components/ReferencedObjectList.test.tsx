import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { ReferencedObjectList } from "@/components/ReferencedObjectList";

describe("ReferencedObjectList", () => {
  it("links each referenced object to its own page", () => {
    render(
      <ReferencedObjectList
        referencedObjects={[
          { name: "app.film", url: "#/tables/film-373567b4" },
          { name: "app.mood", url: "#/user-defined-types/mood-32512554" },
        ]}
      />,
    );

    expect(screen.getByRole("link", { name: "app.film" })).toHaveAttribute(
      "href",
      "#/tables/film-373567b4",
    );
    expect(screen.getByRole("link", { name: "app.mood" })).toHaveAttribute(
      "href",
      "#/user-defined-types/mood-32512554",
    );
  });
});
