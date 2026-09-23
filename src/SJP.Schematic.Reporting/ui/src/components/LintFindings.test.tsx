import { screen } from "@testing-library/react";
import { describe, expect, it } from "vitest";

import { LintFindings } from "@/components/LintFindings";
import { useSummary } from "@/hooks/useReportData";
import { lintMessage as message, lintSummary } from "@/test/lint";
import { failToLoad, renderWithClient } from "@/test/utils";
import type { LintMessage } from "@/types/report";

function renderFindings(messages?: LintMessage[]) {
  return renderWithClient(<LintFindings objectUrl="#/tables/actor-1" />, {
    data: { summaries: messages === undefined ? {} : { lint: lintSummary(messages) } },
  });
}

/** Shows the lint summary query's status, so a test can wait for it to settle. */
function LintQueryStatus() {
  const { status } = useSummary("lint");
  return <output>{status}</output>;
}

describe("LintFindings", () => {
  it("renders the findings raised against the given object", () => {
    renderFindings([message()]);

    expect(screen.getByText("Lint")).toBeInTheDocument();
    expect(screen.getByText("The table actor has no primary key.")).toBeInTheDocument();
  });

  it("ignores findings belonging to other objects", () => {
    const { container } = renderFindings([
      message({ objectUrl: "#/tables/film-2", message: "a film problem" }),
    ]);

    expect(container).toBeEmptyDOMElement();
  });

  it("renders nothing when the object is clean", () => {
    const { container } = renderFindings([]);

    expect(container).toBeEmptyDOMElement();
  });

  it("orders findings most severe first", () => {
    renderFindings([
      message({ level: "Information", message: "an informational finding" }),
      message({ level: "Error", message: "an error finding" }),
    ]);

    const rendered = screen.getAllByRole("listitem").map((li) => li.textContent ?? "");
    expect(rendered[0]).toContain("an error finding");
    expect(rendered[1]).toContain("an informational finding");
  });

  it("links each finding to its rule on the lint page", () => {
    renderFindings([message({ ruleId: "SCHEMATIC0009" })]);

    expect(screen.getByRole("link", { name: "Missing primary key" })).toHaveAttribute(
      "href",
      "#/lint?rule=SCHEMATIC0009",
    );
  });

  it("stays silent while the lint summary is still loading", () => {
    const { container } = renderFindings();

    expect(container).toBeEmptyDOMElement();
  });

  it("stays silent when the lint summary fails to load", async () => {
    failToLoad(new Error("network down"));

    // Lint is supplementary here — a failure must not put an error banner on an
    // otherwise-working detail page.
    renderWithClient(
      <>
        <div data-testid="findings">
          <LintFindings objectUrl="#/tables/actor-1" />
        </div>
        <LintQueryStatus />
      </>,
    );

    // Empty before the failure lands proves nothing, so wait for the query to have failed.
    expect(await screen.findByText("error")).toBeInTheDocument();
    expect(screen.getByTestId("findings")).toBeEmptyDOMElement();
  });
});
