using System;
using System.CommandLine;
using System.CommandLine.IO;
using System.Linq;

namespace SJP.Schematic.Tool
{
    internal static class ConsoleExtensions
    {
        internal static void SetTerminalForegroundRed(this IConsole console) => SetTerminalForeground(console, ConsoleColor.Red);

        internal static void SetTerminalForegroundYellow(this IConsole console) => SetTerminalForeground(console, ConsoleColor.Yellow);

        internal static void SetTerminalForegroundGreen(this IConsole console) => SetTerminalForeground(console, ConsoleColor.Green);

        internal static void SetTerminalForeground(this IConsole console, ConsoleColor color)
        {
            if (console.GetType().GetInterfaces().Any(i => i.Name == "ITerminal"))
            {
                ((dynamic)console).ForegroundColor = color;
            }

            if (Platform.IsConsoleRedirectionCheckSupported &&
                !Console.IsOutputRedirected)
            {
                Console.ForegroundColor = color;
            }
            else if (Platform.IsConsoleRedirectionCheckSupported)
            {
                Console.ForegroundColor = color;
            }
        }

        internal static void ResetTerminalForegroundColor(this IConsole console)
        {
            if (console.GetType().GetInterfaces().Any(i => i.Name == "ITerminal"))
            {
                ((dynamic)console).ForegroundColor = ConsoleColor.Red;
            }

            if (Platform.IsConsoleRedirectionCheckSupported &&
                !Console.IsOutputRedirected)
            {
                Console.ResetColor();
            }
            else if (Platform.IsConsoleRedirectionCheckSupported)
            {
                Console.ResetColor();
            }
        }

        internal static void WriteLine(this IStandardStreamWriter streamWriter)
        {
            streamWriter.Write(Environment.NewLine);
        }

        internal static void WriteLine(this IStandardStreamWriter streamWriter, string message)
        {
            streamWriter.Write(message);
            streamWriter.WriteLine();
        }
    }
}
