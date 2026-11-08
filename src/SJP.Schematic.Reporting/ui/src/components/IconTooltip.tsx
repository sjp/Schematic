import { useState } from "react";

import { Tooltip, TooltipContent, TooltipTrigger } from "@/components/ui/tooltip";

/**
 * Wraps a small status icon (key membership, nullability, …) in a tooltip and
 * makes it keyboard-focusable, so the meaning is discoverable by both pointer
 * and keyboard users. The visible icon should still carry an `aria-label` for
 * screen readers; this adds an on-screen explanation.
 *
 * Radix tooltips only open on hover and focus, and close on click, which leaves
 * touch screens with no way to see them. Taps toggle the tooltip instead; a tap
 * elsewhere still dismisses it.
 */
export function IconTooltip({
  label,
  children,
}: {
  label: React.ReactNode;
  children: React.ReactNode;
}) {
  const [open, setOpen] = useState(false);
  return (
    <Tooltip open={open} onOpenChange={setOpen}>
      <TooltipTrigger asChild>
        <button
          type="button"
          onClick={(e) => {
            // Stops Radix's own close-on-click, which runs after this handler.
            e.preventDefault();
            setOpen((o) => !o);
          }}
          className="inline-flex cursor-help rounded-sm outline-none focus-visible:ring-2 focus-visible:ring-ring"
        >
          {children}
        </button>
      </TooltipTrigger>
      <TooltipContent>{label}</TooltipContent>
    </Tooltip>
  );
}
