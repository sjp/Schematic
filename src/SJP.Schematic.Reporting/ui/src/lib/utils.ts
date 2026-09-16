import { clsx, type ClassValue } from "clsx";
import { twMerge } from "tailwind-merge";

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/** Narrows an optional string to one that carries text, i.e. is neither absent nor empty. */
export function hasText(value: string | null | undefined): value is string {
  return value !== null && value !== undefined && value !== "";
}
