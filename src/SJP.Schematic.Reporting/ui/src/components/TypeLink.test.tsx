import { render, screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { TypeLink } from "@/components/TypeLink";

describe("TypeLink", () => {
  it("links a user-defined type to its own page", () => {
    render(<TypeLink type="app.mood" typeUrl="#/user-defined-types/mood-32512554" />);

    expect(screen.getByRole("link", { name: "app.mood" })).toHaveAttribute(
      "href",
      "#/user-defined-types/mood-32512554",
    );
  });

  it("renders a type with no page as plain text", () => {
    render(<TypeLink type="integer" />);

    expect(screen.queryByRole("link")).toBeNull();
    expect(screen.getByText("integer")).toBeInTheDocument();
  });
});
