import { describe, expect, it } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
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

    const trigger = icon.closest("span[tabindex]");
    expect(trigger).toHaveAttribute("tabindex", "0");
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
});
