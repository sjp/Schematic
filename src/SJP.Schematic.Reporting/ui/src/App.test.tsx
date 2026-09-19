import { render, screen } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";

import App from "@/App";
import { useColorScheme } from "@/hooks/useColorScheme";
import { router } from "@/router";

vi.mock("@/hooks/useColorScheme", () => ({ useColorScheme: vi.fn<typeof useColorScheme>() }));

// The real provider would mount the whole route tree; this checks only that the app hands it the
// app's own router. The rest of the package stays real, because building the route tree needs it.
vi.mock("@tanstack/react-router", async (importOriginal) => ({
  ...(await importOriginal<typeof import("@tanstack/react-router")>()),
  RouterProvider: ({ router: provided }: { router: unknown }) => (
    <div data-testid="router-provider" data-is-app-router={String(provided === router)} />
  ),
}));

describe("App", () => {
  it("renders the app's router and follows the OS colour scheme", () => {
    render(<App />);

    expect(screen.getByTestId("router-provider")).toHaveAttribute("data-is-app-router", "true");
    expect(useColorScheme).toHaveBeenCalled();
  });
});
