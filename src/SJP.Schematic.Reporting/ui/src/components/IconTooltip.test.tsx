import { fireEvent, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";

import { IconTooltip } from "@/components/IconTooltip";

describe("IconTooltip", () => {
  it("renders its children inside a keyboard-focusable trigger", () => {
    render(
      <IconTooltip label="Primary key">
        <span data-testid="icon">PK</span>
      </IconTooltip>,
    );

    const icon = screen.getByTestId("icon");
    expect(icon).toBeInTheDocument();

    const trigger = icon.closest("button");
    expect(trigger).toBeInTheDocument();
  });

  it("shows the label content when the trigger is focused", async () => {
    const user = userEvent.setup();
    render(
      <IconTooltip label="Primary key">
        <span data-testid="icon">PK</span>
      </IconTooltip>,
    );

    await user.tab();

    const tooltip = await screen.findByRole("tooltip");
    expect(tooltip).toHaveTextContent("Primary key");
  });

  it("toggles the label on a tap, since touch screens never hover", () => {
    render(
      <IconTooltip label="Primary key">
        <span data-testid="icon">PK</span>
      </IconTooltip>,
    );
    // A bare click event, with no hover or focus first, is what a tap on a touch screen produces.
    const trigger = screen.getByRole("button");

    fireEvent.click(trigger);
    expect(screen.getByRole("tooltip")).toHaveTextContent("Primary key");

    fireEvent.click(trigger);
    expect(screen.queryByRole("tooltip")).not.toBeInTheDocument();
  });
});
