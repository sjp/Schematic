import {
  Tooltip,
  TooltipContent,
  TooltipTrigger,
} from "@/components/ui/tooltip";

/**
 * Wraps a small status icon (key membership, nullability, …) in a tooltip and
 * makes it keyboard-focusable, so the meaning is discoverable by both pointer
 * and keyboard users. The visible icon should still carry an `aria-label` for
 * screen readers; this adds an on-screen explanation.
 */
export function IconTooltip({
  label,
  children,
}: {
  label: React.ReactNode;
  children: React.ReactNode;
}) {
  return (
    <Tooltip>
      <TooltipTrigger asChild>
        <span
          tabIndex={0}
          className="focus-visible:ring-ring inline-flex cursor-help rounded-sm outline-none focus-visible:ring-2"
        >
          {children}
        </span>
      </TooltipTrigger>
      <TooltipContent>{label}</TooltipContent>
    </Tooltip>
  );
}
