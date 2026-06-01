/**
 * Renders a pre-generated diagram SVG. An `<object>` (rather than `<img>`) keeps the SVG's
 * internal structure live for the cross-link/zoom work in issue 13, and loads correctly over
 * both `file://` and `http://` from the report-relative `data/diagrams/<id>.svg` path.
 */
export function Diagram({ src, title }: { src: string; title?: string }) {
  return (
    <div className="bg-card overflow-auto rounded-md border p-4">
      <object
        type="image/svg+xml"
        data={src}
        aria-label={title ?? 'Schema diagram'}
        className="mx-auto block max-w-full"
      >
        <a href={src}>Open diagram</a>
      </object>
    </div>
  )
}
