import { useState } from 'react'
import { Minus, Plus, RotateCcw } from 'lucide-react'
import { Button } from '@/components/ui/button'

const MIN_ZOOM = 0.5
const MAX_ZOOM = 4
const ZOOM_STEP = 0.25

/**
 * Renders a pre-generated diagram SVG. An `<object>` (rather than `<img>`) keeps the SVG's internal
 * `<a>` cross-links live and loads correctly over both `file://` and `http://` from the
 * report-relative `data/diagrams/<id>.svg` path. Nodes link back to `../../index.html#/tables/<key>`
 * with `target="_top"`, so clicking one drives the SPA's hash router.
 *
 * Zoom is applied by widening the inner wrapper; the scrollable container then provides panning. We
 * deliberately avoid a drag overlay, which would intercept (and break) the in-SVG link clicks.
 */
export function Diagram({ src, title }: { src: string; title?: string }) {
  const [zoom, setZoom] = useState(1)

  const zoomIn = () =>
    setZoom((z) => Math.min(MAX_ZOOM, +(z + ZOOM_STEP).toFixed(2)))
  const zoomOut = () =>
    setZoom((z) => Math.max(MIN_ZOOM, +(z - ZOOM_STEP).toFixed(2)))
  const reset = () => setZoom(1)

  return (
    <div className="bg-card relative rounded-md border">
      <div className="absolute top-2 right-2 z-10 flex items-center gap-1 rounded-md border bg-background/80 p-1 backdrop-blur">
        <Button
          variant="ghost"
          size="icon"
          className="size-7"
          onClick={zoomOut}
          disabled={zoom <= MIN_ZOOM}
          aria-label="Zoom out"
        >
          <Minus className="size-4" />
        </Button>
        <span className="text-muted-foreground w-10 text-center text-xs tabular-nums">
          {Math.round(zoom * 100)}%
        </span>
        <Button
          variant="ghost"
          size="icon"
          className="size-7"
          onClick={zoomIn}
          disabled={zoom >= MAX_ZOOM}
          aria-label="Zoom in"
        >
          <Plus className="size-4" />
        </Button>
        <Button
          variant="ghost"
          size="icon"
          className="size-7"
          onClick={reset}
          disabled={zoom === 1}
          aria-label="Reset zoom"
        >
          <RotateCcw className="size-4" />
        </Button>
      </div>
      <div className="overflow-auto p-4">
        <div className="mx-auto" style={{ width: `${zoom * 100}%` }}>
          <object
            type="image/svg+xml"
            data={src}
            aria-label={title ?? 'Schema diagram'}
            className="block w-full"
          >
            <a href={src}>Open diagram</a>
          </object>
        </div>
      </div>
    </div>
  )
}
