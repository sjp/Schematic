import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";
import "@testing-library/jest-dom/vitest";

// With `test.globals: false`, `@testing-library/react`'s own auto-cleanup (which
// detects a global `afterEach`) never registers, so each test would render on top
// of the last. Register it explicitly instead.
afterEach(() => {
  cleanup();
});

// jsdom does not implement matchMedia; useColorScheme and RootLayout both depend on it.
if (typeof window.matchMedia !== "function") {
  window.matchMedia = (query: string): MediaQueryList => ({
    matches: false,
    media: query,
    onchange: null,
    // `MediaQueryList` still declares the superseded listener pair, so a conforming stub has to
    // carry it even though nothing here calls it.
    /* oxlint-disable typescript/no-deprecated */
    addListener: () => {},
    removeListener: () => {},
    /* oxlint-enable typescript/no-deprecated */
    addEventListener: () => {},
    removeEventListener: () => {},
    dispatchEvent: () => false,
  });
}

// jsdom has no ResizeObserver; Radix UI primitives (Tooltip, Dialog) measure elements with it.
if (typeof window.ResizeObserver !== "function") {
  window.ResizeObserver = class ResizeObserver {
    observe() {}
    unobserve() {}
    disconnect() {}
  };
}

// jsdom does not implement pointer capture, which Radix UI primitives call unconditionally.
if (typeof Element.prototype.hasPointerCapture !== "function") {
  Element.prototype.hasPointerCapture = () => false;
}
if (typeof Element.prototype.setPointerCapture !== "function") {
  Element.prototype.setPointerCapture = () => {};
}
if (typeof Element.prototype.releasePointerCapture !== "function") {
  Element.prototype.releasePointerCapture = () => {};
}
if (typeof Element.prototype.scrollIntoView !== "function") {
  Element.prototype.scrollIntoView = () => {};
}
