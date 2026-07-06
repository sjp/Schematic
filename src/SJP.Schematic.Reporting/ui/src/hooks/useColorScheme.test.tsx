import { afterEach, describe, expect, it, vi } from "vitest";
import { renderHook } from "@testing-library/react";
import { useColorScheme } from "@/hooks/useColorScheme";

function mockMatchMedia() {
  const listeners = new Set<(e: MediaQueryListEvent) => void>();
  const removeEventListener = vi.fn(
    (_: "change", listener: (e: MediaQueryListEvent) => void) => {
      listeners.delete(listener);
    },
  );
  const addEventListener = vi.fn(
    (_: "change", listener: (e: MediaQueryListEvent) => void) => {
      listeners.add(listener);
    },
  );
  const matchMedia = vi.fn().mockReturnValue({
    matches: false,
    addEventListener,
    removeEventListener,
  });
  vi.stubGlobal("matchMedia", matchMedia);

  return {
    matchMedia,
    addEventListener,
    removeEventListener,
    fireChange: (matches: boolean) => {
      for (const listener of listeners) {
        listener({ matches } as MediaQueryListEvent);
      }
    },
  };
}

describe("useColorScheme", () => {
  afterEach(() => {
    vi.unstubAllGlobals();
    document.documentElement.classList.remove("dark");
  });

  it("queries prefers-color-scheme: dark and subscribes to changes", () => {
    const { matchMedia, addEventListener } = mockMatchMedia();

    renderHook(() => useColorScheme());

    expect(matchMedia).toHaveBeenCalledWith("(prefers-color-scheme: dark)");
    expect(addEventListener).toHaveBeenCalledWith(
      "change",
      expect.any(Function),
    );
  });

  it("toggles the dark class when the OS preference changes", () => {
    const { fireChange } = mockMatchMedia();
    renderHook(() => useColorScheme());

    fireChange(true);
    expect(document.documentElement.classList.contains("dark")).toBe(true);

    fireChange(false);
    expect(document.documentElement.classList.contains("dark")).toBe(false);
  });

  it("unsubscribes on unmount", () => {
    const { removeEventListener } = mockMatchMedia();
    const { unmount } = renderHook(() => useColorScheme());

    unmount();

    expect(removeEventListener).toHaveBeenCalledWith(
      "change",
      expect.any(Function),
    );
  });
});
