import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import { NotFound } from "@/components/layout/NotFound";

describe("NotFound", () => {
  it("renders a heading and explanatory message", () => {
    render(<NotFound />);
    expect(
      screen.getByRole("heading", { name: "Not found" }),
    ).toBeInTheDocument();
    expect(
      screen.getByText(
        "This page has not been built yet, or the link is invalid.",
      ),
    ).toBeInTheDocument();
  });
});
