import path from "node:path";
import { defineConfig, type Plugin } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from "@tailwindcss/vite";

/**
 * Rewrites the generated `index.html` so it opens by double-click from disk
 * (`file://`):
 *  - Strip `crossorigin`: it makes the browser refuse same-origin-but-opaque
 *    (`file://`) assets.
 *  - Swap `type="module"` for `defer` on the entry script: ES-module scripts
 *    are CORS-blocked over `file://`. The bundle is built as a single IIFE, so
 *    it runs correctly as a classic script; `defer` preserves the module
 *    script's implicit deferred execution (it sits in `<head>`, so it must not
 *    run before `#root` is parsed).
 */
function fileProtocolHtml(): Plugin {
  return {
    name: "file-protocol-html",
    enforce: "post",
    transformIndexHtml(html) {
      return html
        .replace(/\s+crossorigin(=("|')[^"']*\2)?/g, "")
        .replace(/<script\s+type="module"/g, "<script defer");
    },
  };
}

// https://vite.dev/config/
export default defineConfig({
  // Relative asset paths are mandatory for opening the report from `file://`.
  base: "./",
  plugins: [react(), tailwindcss(), fileProtocolHtml()],
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
  build: {
    // Disable module preload polyfill — there is a single self-contained bundle,
    // and the polyfill emits a module script that breaks `file://` loading.
    modulePreload: { polyfill: false },
    cssCodeSplit: false,
    rollupOptions: {
      output: {
        // A single self-contained IIFE bundle so the app loads as a classic
        // script over both `file://` and `http://`.
        format: "iife",
        inlineDynamicImports: true,
        entryFileNames: "assets/app-[hash].js",
        assetFileNames: "assets/app-[hash][extname]",
      },
    },
  },
});
