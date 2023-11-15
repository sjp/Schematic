using System;
using System.Runtime.Serialization;

namespace SJP.Schematic.Tool;

[Serializable]
internal class CommandException : Exception
{
    public CommandException()
    {
    }

    public CommandException(string message)
        : base(message)
    {
    }

    public CommandException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}