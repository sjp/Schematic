import { useMemo } from "react";
import * as Dialog from "@radix-ui/react-dialog";
import { Command } from "cmdk";
import {
  Columns3,
  Eye,
  ListOrdered,
  Replace,
  Search as SearchIcon,
  SquareFunction,
  Table2,
} from "lucide-react";
import { useSummary } from "@/hooks/useReportData";
import type { SearchEntry, SearchSummary } from "@/types/report";

// Stable display order for the grouped result sections.
const TYPE_ORDER = [
  "Table",
  "View",
  "Sequence",
  "Synonym",
  "Routine",
  "Column",
];

const TYPE_ICON: Record<string, typeof Table2> = {
  Table: Table2,
  View: Eye,
  Sequence: ListOrdered,
  Synonym: Replace,
  Routine: SquareFunction,
  Column: Columns3,
};

export function SearchCommand({
  open,
  onOpenChange,
}: {
  open: boolean;
  onOpenChange: (open: boolean) => void;
}) {
  // The query is cheap and cached; on file:// it resolves synchronously from the bundle.
  const { data } = useSummary<SearchSummary>("search");

  const grouped = useMemo(() => {
    const byType = new Map<string, SearchEntry[]>();
    for (const entry of data?.entries ?? []) {
      const list = byType.get(entry.objectType) ?? [];
      list.push(entry);
      byType.set(entry.objectType, list);
    }
    return [...byType.entries()].sort(
      ([a], [b]) =>
        (TYPE_ORDER.indexOf(a) + 1 || 99) - (TYPE_ORDER.indexOf(b) + 1 || 99),
    );
  }, [data]);

  function go(url: string) {
    onOpenChange(false);
    // url is an absolute hash route (e.g. `#/tables/<key>`); hash history picks it up.
    // Browser navigation from a user-interaction handler, not a React value mutated during render.
    // eslint-disable-next-line react-hooks/immutability
    window.location.hash = url.startsWith("#") ? url.slice(1) : url;
  }

  return (
    <Dialog.Root open={open} onOpenChange={onOpenChange}>
      <Dialog.Portal>
        <Dialog.Overlay className="fixed inset-0 z-40 bg-black/50" />
        <Dialog.Content
          aria-describedby={undefined}
          className="bg-popover text-popover-foreground fixed top-[20%] left-1/2 z-50 w-full max-w-lg -translate-x-1/2 overflow-hidden rounded-lg border shadow-lg"
        >
          {/* Radix requires an accessible title; visually hidden to keep the palette clean. */}
          <Dialog.Title className="sr-only">Search schema</Dialog.Title>
          <Command shouldFilter label="Search schema">
            <div className="flex items-center gap-2 border-b px-3">
              <SearchIcon className="text-muted-foreground size-4 shrink-0" />
              <Command.Input
                placeholder="Search tables, views, columns…"
                className="placeholder:text-muted-foreground h-11 w-full bg-transparent py-3 text-sm outline-none"
              />
            </div>
            <Command.List className="max-h-80 overflow-y-auto p-1">
              <Command.Empty className="text-muted-foreground py-6 text-center text-sm">
                No results found.
              </Command.Empty>
              {grouped.map(([type, items]) => {
                const Icon = TYPE_ICON[type] ?? SearchIcon;
                return (
                  <Command.Group
                    key={type}
                    heading={type}
                    className="text-muted-foreground [&_[cmdk-group-heading]]:text-muted-foreground [&_[cmdk-group-heading]]:px-2 [&_[cmdk-group-heading]]:py-1.5 [&_[cmdk-group-heading]]:text-xs [&_[cmdk-group-heading]]:font-medium"
                  >
                    {items.map((entry, i) => (
                      <Command.Item
                        key={`${type}:${entry.parent ?? ""}:${entry.name}:${entry.url}:${i}`}
                        value={`${entry.name} ${entry.parent ?? ""} ${type}`}
                        onSelect={() => go(entry.url)}
                        className="text-foreground data-[selected=true]:bg-accent data-[selected=true]:text-accent-foreground flex cursor-pointer items-center gap-2 rounded-md px-2 py-1.5 text-sm"
                      >
                        <Icon className="text-muted-foreground size-4 shrink-0" />
                        <span className="truncate">{entry.name}</span>
                        {entry.parent && (
                          <span className="text-muted-foreground ml-auto truncate text-xs">
                            {entry.parent}
                          </span>
                        )}
                      </Command.Item>
                    ))}
                  </Command.Group>
                );
              })}
            </Command.List>
          </Command>
        </Dialog.Content>
      </Dialog.Portal>
    </Dialog.Root>
  );
}
