import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";

import { ConstraintStatus } from "@/components/ConstraintStatus";

describe("ConstraintStatus", () => {
  it("says a validated, non-deferrable constraint is enforced", () => {
    render(<ConstraintStatus isValidated={true} deferrabilityDescription="" />);
    expect(screen.getByText("Enforced")).toBeInTheDocument();
  });

  it("shows the deferrability in place of 'Enforced' when the constraint can be deferred", () => {
    render(
      <ConstraintStatus
        isValidated={true}
        deferrabilityDescription="DEFERRABLE INITIALLY DEFERRED"
      />,
    );
    expect(screen.getByText("DEFERRABLE INITIALLY DEFERRED")).toBeInTheDocument();
    expect(screen.queryByText("Enforced")).not.toBeInTheDocument();
  });

  it("flags an unvalidated constraint", () => {
    render(<ConstraintStatus isValidated={false} deferrabilityDescription="" />);
    expect(screen.getByText("Not validated")).toBeInTheDocument();
  });

  it("lowercases the deferrability when appending it to the unvalidated label", () => {
    render(
      <ConstraintStatus
        isValidated={false}
        deferrabilityDescription="DEFERRABLE INITIALLY IMMEDIATE"
      />,
    );
    expect(screen.getByText("Not validated, deferrable initially immediate")).toBeInTheDocument();
  });

  it("explains via a tooltip why an unvalidated constraint matters", async () => {
    const user = userEvent.setup();
    render(<ConstraintStatus isValidated={false} deferrabilityDescription="" />);

    await user.tab();

    const tooltip = await screen.findByRole("tooltip");
    expect(tooltip).toHaveTextContent("will not rely upon it when planning queries");
  });
});
