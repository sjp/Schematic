import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useDetail } from "@/hooks/useReportData";
import { UserDefinedTypeDetailPage } from "@/routes/user-defined-type-detail";
import { failedQuery, loadedQuery, pendingQuery } from "@/test/queryResult";
import type { UserDefinedTypeDetail } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useDetail: vi.fn<typeof useDetail>(),
}));

// The route reads its param through `getRouteApi` and links with `Link`; stub both so the page can
// render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  getRouteApi: () => ({ useParams: () => ({ typeKey: "postcode-1a2b" }) }),
  Link: ({
    to,
    children,
    className,
  }: {
    to?: string;
    children: React.ReactNode;
    className?: string;
  }) => (
    <a href={to ?? "#"} className={className}>
      {children}
    </a>
  ),
}));

const mockUseDetail = vi.mocked(useDetail<UserDefinedTypeDetail>);

const TYPE: UserDefinedTypeDetail = {
  name: "postcode",
  typeUrl: "#/user-defined-types/postcode-1a2b",
  kind: "Domain",
  baseType: "text",
  isNullable: false,
  defaultValue: "'0000'",
  definition: "CREATE DOMAIN postcode AS text",
  enumValues: [],
  enumValuesCount: 0,
  attributes: [],
  attributesCount: 0,
  checks: [],
  checksCount: 0,
};

describe("UserDefinedTypeDetailPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseDetail.mockReturnValue(pendingQuery());

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseDetail.mockReturnValue(failedQuery(new Error("boom")));

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByText("Failed to load type: boom")).toBeInTheDocument();
  });

  it("heads the page with the type's name and kind, under a link back to the list", () => {
    mockUseDetail.mockReturnValue(loadedQuery(TYPE));

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByRole("heading", { name: "postcode" })).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "User-Defined Types" })).toHaveAttribute(
      "href",
      "/user-defined-types",
    );
    // Once as the header badge, once as the Kind property.
    expect(screen.getAllByText("Domain")).toHaveLength(2);
  });

  it("shows the base type, nullability and default", () => {
    mockUseDetail.mockReturnValue(loadedQuery(TYPE));

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByText("text")).toBeInTheDocument();
    expect(screen.getByText("No")).toBeInTheDocument();
    expect(screen.getByText("'0000'")).toBeInTheDocument();
  });

  it("links a base type that is itself user-defined", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({ ...TYPE, baseTypeUrl: "#/user-defined-types/text_alias-9f8e" }),
    );

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByRole("link", { name: "text" })).toHaveAttribute(
      "href",
      "#/user-defined-types/text_alias-9f8e",
    );
  });

  it("shows an em dash for every property the database does not report", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...TYPE,
        kind: "",
        baseType: "",
        defaultValue: "",
        isNullable: true,
      }),
    );

    render(<UserDefinedTypeDetailPage />);
    // Kind, Base Type and Default; Nullable reads "Yes".
    expect(screen.getAllByText("—")).toHaveLength(3);
    expect(screen.getByText("Yes")).toBeInTheDocument();
    // With no kind there is no header badge either.
    expect(screen.queryByText("Domain")).not.toBeInTheDocument();
  });

  it("leaves out the sections a plain domain has nothing to fill them with", () => {
    mockUseDetail.mockReturnValue(loadedQuery(TYPE));

    render(<UserDefinedTypeDetailPage />);
    expect(screen.queryByRole("heading", { name: /Values/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Attributes/u })).not.toBeInTheDocument();
    expect(screen.queryByRole("heading", { name: /Check Constraints/u })).not.toBeInTheDocument();
  });

  it("lists an enum's values", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({ ...TYPE, enumValues: ["G", "PG", "NC-17"], enumValuesCount: 3 }),
    );

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByRole("heading", { name: "Values(3)" })).toBeInTheDocument();
    for (const value of ["G", "PG", "NC-17"]) {
      expect(screen.getByText(value)).toBeInTheDocument();
    }
  });

  it("lists a composite type's attributes, linking a user-defined attribute type", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...TYPE,
        kind: "Composite",
        attributes: [
          {
            ordinal: 1,
            attributeName: "street",
            isNullable: true,
            type: "text",
            defaultValue: "",
          },
          {
            ordinal: 2,
            attributeName: "zip",
            isNullable: false,
            type: "postcode",
            typeUrl: "#/user-defined-types/postcode-1a2b",
            defaultValue: "'0000'",
          },
        ],
        attributesCount: 2,
      }),
    );

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByRole("heading", { name: "Attributes(2)" })).toBeInTheDocument();
    expect(screen.getByText("street")).toBeInTheDocument();
    expect(screen.getByRole("link", { name: "postcode" })).toHaveAttribute(
      "href",
      "#/user-defined-types/postcode-1a2b",
    );
    expect(screen.getByLabelText("Nullable")).toBeInTheDocument();
    expect(screen.getByLabelText("Not nullable")).toBeInTheDocument();
  });

  it("lists the checks a value must satisfy, with an em dash for an unnamed one", () => {
    mockUseDetail.mockReturnValue(
      loadedQuery({
        ...TYPE,
        checks: [
          { constraintName: "postcode_length", definition: "length(VALUE) = 4" },
          { constraintName: "", definition: "VALUE ~ '^[0-9]+$'" },
        ],
        checksCount: 2,
      }),
    );

    render(<UserDefinedTypeDetailPage />);
    expect(screen.getByRole("heading", { name: "Check Constraints(2)" })).toBeInTheDocument();
    expect(screen.getByText("postcode_length")).toBeInTheDocument();
    expect(screen.getByText("length(VALUE) = 4")).toBeInTheDocument();
    expect(screen.getByText("VALUE ~ '^[0-9]+$'")).toBeInTheDocument();
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("shows the definition when the database reports one, and omits the section otherwise", () => {
    mockUseDetail.mockReturnValue(loadedQuery(TYPE));

    const { unmount } = render(<UserDefinedTypeDetailPage />);
    expect(screen.getByRole("heading", { name: "Definition" })).toBeInTheDocument();
    expect(screen.getByText("CREATE DOMAIN postcode AS text")).toBeInTheDocument();
    unmount();

    mockUseDetail.mockReturnValue(loadedQuery({ ...TYPE, definition: "" }));

    render(<UserDefinedTypeDetailPage />);
    expect(screen.queryByRole("heading", { name: "Definition" })).not.toBeInTheDocument();
  });
});
