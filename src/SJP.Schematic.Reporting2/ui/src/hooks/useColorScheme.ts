import { useEffect } from 'react'

const DARK_QUERY = '(prefers-color-scheme: dark)'

/** Toggle the `dark` class on the document root to match `prefers-color-scheme`. */
function applyColorScheme(isDark: boolean) {
  document.documentElement.classList.toggle('dark', isDark)
}

/**
 * Follow the operating system's light/dark preference, updating live if the
 * user changes it while the report is open. The initial class is set
 * synchronously by an inline script in index.html to avoid a flash of the
 * wrong theme; this keeps it in sync afterwards.
 */
export function useColorScheme() {
  useEffect(() => {
    const media = window.matchMedia(DARK_QUERY)
    applyColorScheme(media.matches)

    const onChange = (e: MediaQueryListEvent) => applyColorScheme(e.matches)
    media.addEventListener('change', onChange)
    return () => media.removeEventListener('change', onChange)
  }, [])
}
