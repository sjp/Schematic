import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import { useSummary } from "@/hooks/useReportData";
import { UserDefinedTypesPage } from "@/routes/user-defined-types";
import type { UserDefinedTypesSummary } from "@/types/report";

vi.mock("@/hooks/useReportData", () => ({
  useSummary: vi.fn(),
}));

// `user-defined-types.tsx` only imports `Link` from this package; stub it as a plain anchor
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

const mockUseSummary = vi.mocked(useSummary<UserDefinedTypesSummary>);

const TYPE = {
  name: "public.mood",
  typeUrl: "#/user-defined-types/mood-1a2b3c4d",
  kind: "Enum",
  baseType: "",
  isNullable: true,
  attributesCount: 0,
  enumValuesCount: 3,
};

describe("UserDefinedTypesPage", () => {
  it("shows a loading indicator while pending", () => {
    mockUseSummary.mockReturnValue({
      isPending: true,
      isError: false,
      data: undefined,
      error: null,
    } as never);

    render(<UserDefinedTypesPage />);
    expect(screen.getByText("Loading…")).toBeInTheDocument();
  });

  it("shows the error message on failure", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: true,
      data: undefined,
      error: new Error("boom"),
    } as never);

    render(<UserDefinedTypesPage />);
    expect(screen.getByText("Failed to load user-defined types: boom")).toBeInTheDocument();
  });

  it("links each row's name to its type detail route, derived from the hash url", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: { userDefinedTypesCount: 1, allUserDefinedTypes: [TYPE] },
      error: null,
    } as never);

    render(<UserDefinedTypesPage />);
    const link = screen.getByRole("link", { name: "public.mood" });
    expect(link).toHaveAttribute("href", "/user-defined-types/mood-1a2b3c4d");
  });

  it("shows an em dash for a type that is not defined in terms of another", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: { userDefinedTypesCount: 1, allUserDefinedTypes: [TYPE] },
      error: null,
    } as never);

    render(<UserDefinedTypesPage />);
    expect(screen.getByText("Enum")).toBeInTheDocument();
    expect(screen.getByText("—")).toBeInTheDocument();
  });

  it("shows the types count in the heading", () => {
    mockUseSummary.mockReturnValue({
      isPending: false,
      isError: false,
      data: { userDefinedTypesCount: 3, allUserDefinedTypes: [] },
      error: null,
    } as never);

    render(<UserDefinedTypesPage />);
    expect(screen.getByText("(3)")).toBeInTheDocument();
  });
});
