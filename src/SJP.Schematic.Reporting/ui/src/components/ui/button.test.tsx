import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { Button } from "@/components/ui/button";

describe("Button", () => {
  it("renders a button element with the default variant/size classes", () => {
    render(<Button>Click me</Button>);
    const button = screen.getByRole("button", { name: "Click me" });
    expect(button).toHaveClass("bg-primary", "h-9");
  });

  it("applies variant and size classes", () => {
    render(
      <Button variant="outline" size="icon" aria-label="icon button">
        X
      </Button>,
    );
    const button = screen.getByRole("button", { name: "icon button" });
    expect(button).toHaveClass("border", "size-9");
    expect(button).not.toHaveClass("bg-primary");
  });

  it("forwards arbitrary props such as onClick and disabled", async () => {
    const user = userEvent.setup();
    const onClick = vi.fn();
    render(
      <Button onClick={onClick} disabled>
        Disabled
      </Button>,
    );

    const button = screen.getByRole("button", { name: "Disabled" });
    expect(button).toBeDisabled();
    await user.click(button);
    expect(onClick).not.toHaveBeenCalled();
  });
});
