using System;

namespace SJP.Schematic.Tool
{
    internal static class Platform
    {
        public static bool IsConsoleRedirectionCheckSupported => _isConsoleRedirectionCheckSupported.Value;

        private static readonly Lazy<bool> _isConsoleRedirectionCheckSupported = new(static () =>
        {
            try
            {
                _ = Console.IsOutputRedirected;
                return true;
            }
            catch (PlatformNotSupportedException)
            {
                return false;
            }
        });
    }
}
