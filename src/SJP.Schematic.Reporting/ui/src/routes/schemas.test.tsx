import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { SchemasPage } from "@/routes/schemas";
import type { SchemasSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn(),
}));

// `schemas.tsx` only imports `Link` from this package; stub it as a plain anchor
// so the route can render without a real TanStack Router context.
vi.mock("@tanstack/react-router", () => ({
  Link: ({
    to,
    params,
    children,
    className,
  }: {
    to: string;
    params: Record<string, string>;
    children: React.ReactNode;
    className?: string;
  }) => {
    const href = Object.entries(params).reduce(
      (path, [key, value]) => path.replace(`$${key}`, value),
      to,
    );
    return (
      <a href={href} className={className}>
        {children}
      </a>
    );
  },
}));

const mockUseSummary = vi.mocked(useSummary<SchemasSummary>);

const SCHEMA = {
  name: "public",
  schemaUrl: "#/schemas/public-1a2b3c4d",
  owner: "postgres",
  isDefault: true,
  isSystem: false,
  tablesCount: 4,
  viewsCount: 1,
  sequencesCount: 2,
  synonymsCount: 0,
  routinesCount: 3,
  userDefinedTypesCount: 1,
  objectCount: 11,
};

describe("SchemasPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue({
      isPending: true,
      isError: false,
      data: undefined,
      error: null,
    } as never);

    render(<SchemasPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: true,
      data: undefined,
      error: new Error("boom"),
    } as never);

    render(<SchemasPage />);
    expect(screen.getByText("Failed to load schemas: boom")).toBeInTheDocument();
  });

  it("links each row's name to its schema detail route, derived from the hash url", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: { schemasCount: 1, allSchemas: [SCHEMA] },
      error: null,
    } as never);

    render(<SchemasPage />);
    const link = screen.getByRole("link", { name: "public" });
    expect(link).toHaveAttribute("href", "/schemas/public-1a2b3c4d");
  });

  it("marks the default schema and shows an em dash for an unrecorded owner", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: {
        schemasCount: 2,
        allSchemas: [
          SCHEMA,
          {
            ...SCHEMA,
            name: "sys",
            schemaUrl: "#/schemas/sys-5e6f",
            owner: "",
            isDefault: false,
            isSystem: true,
          },
        ],
      },
      error: null,
    } as never);

    render(<SchemasPage />);
    expect(screen.getByText("default")).toBeInTheDocument();
    expect(screen.getByText("system")).toBeInTheDocument();
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("shows the schemas count in the heading", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: { schemasCount: 3, allSchemas: [] },
      error: null,
    } as never);

    render(<SchemasPage />);
    expect(screen.getByText("(3)")).toBeInTheDocument();
  });
});
