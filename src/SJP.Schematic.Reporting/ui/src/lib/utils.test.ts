import { describe, expect, it } from "vitest";

import { cn, hasText } from "@/lib/utils";

describe("cn", () => {
  it("joins plain class names", () => {
    expect(cn("a", "b")).toBe("a b");
  });

  it("drops falsy values", () => {
    expect(cn("a", false, undefined, null, "", "b")).toBe("a b");
  });

  it("flattens arrays and objects", () => {
    expect(cn(["a", "b"], { c: true, d: false })).toBe("a b c");
  });

  it("resolves conflicting tailwind classes, keeping the last one", () => {
    expect(cn("px-2 py-1", "px-4")).toBe("py-1 px-4");
  });
});

describe("hasText", () => {
  it("accepts a string with content", () => {
    expect(hasText("a")).toBe(true);
    expect(hasText(" ")).toBe(true);
  });

  it("rejects an absent or empty string", () => {
    expect(hasText(undefined)).toBe(false);
    expect(hasText(null)).toBe(false);
    expect(hasText("")).toBe(false);
  });
});
