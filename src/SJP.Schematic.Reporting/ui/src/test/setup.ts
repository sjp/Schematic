import { cleanup } from "@testing-library/react";
import { afterEach, beforeEach, vi } from "vitest";
import "@testing-library/jest-dom/vitest";

// With `test.globals: false`, `@testing-library/react`'s own auto-cleanup (which
// detects a global `afterEach`) never registers, so each test would render on top
// of the last. Register it explicitly instead.
afterEach(() => {
  cleanup();
});

// Report payloads a test has not seeded are fetched for real (see `test/utils.tsx`). Never answer,
// so such a payload stays pending rather than reaching for a server; `unstubGlobals` undoes this,
// and any stub a test makes over it, after each test.
beforeEach(() => {
  vi.stubGlobal("fetch", () => new Promise<never>(() => {}));
});

// happy-dom has no UA stylesheet for the text-level elements, so `getComputedStyle(el).display`
// comes back as "" rather than the initial "inline". `dom-accessibility-api` — which backs Testing
// Library's `{ name }` queries — reads that property to decide whether a child contributes a space
// to the accumulated name, so without this `<h2>Indexes<span>(1)</span></h2>` is named
// "Indexes (1)" rather than the "Indexes(1)" a real browser computes.
const inlineDefaults = document.createElement("style");
inlineDefaults.textContent =
  "a,abbr,b,bdi,bdo,cite,code,data,dfn,em,i,kbd,label,mark,output,q,s,samp,small,span,strong,sub,sup,time,u,var{display:inline}";
document.head.append(inlineDefaults);
