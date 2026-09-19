import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";

import { IndexStatus } from "@/components/IndexStatus";

const USABLE = { isEnabled: true, isValid: true, isVisible: true };

describe("IndexStatus", () => {
  it("says an index in none of the unusable states is usable", () => {
    render(<IndexStatus {...USABLE} />);
    expect(screen.getByText("Usable")).toBeInTheDocument();
  });

  it.each([
    ["isEnabled", { ...USABLE, isEnabled: false }, "Disabled"],
    ["isValid", { ...USABLE, isValid: false }, "Invalid"],
    ["isVisible", { ...USABLE, isVisible: false }, "Invisible"],
  ])("names the reason when %s is false", (_flag, props, expected) => {
    render(<IndexStatus {...props} />);
    expect(screen.getByText(expected)).toBeInTheDocument();
  });

  it("lists every reason an index is unusable, in a fixed order", () => {
    render(<IndexStatus isEnabled={false} isValid={false} isVisible={false} />);
    expect(screen.getByText("Disabled, Invalid, Invisible")).toBeInTheDocument();
  });

  it("explains via a tooltip that the planner will skip the index", async () => {
    const user = userEvent.setup();
    render(<IndexStatus isEnabled={false} isValid={true} isVisible={false} />);

    await user.tab();

    const tooltip = await screen.findByRole("tooltip");
    expect(tooltip).toHaveTextContent(
      "The query planner will not use this index: disabled, invisible",
    );
  });
});
