using System;
using EnumsNET;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Lint
{
    public class RuleMessage : IRuleMessage
    {
        public RuleMessage(string title, RuleLevel level, string message)
        {
            if (title.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(title));
            if (!level.IsValid())
                throw new ArgumentException($"The { nameof(RuleLevel) } provided must be a valid enum.", nameof(level));
            if (message.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(message));

            Title = title;
            Level = level;
            Message = message;
        }

        public string Title { get; }

        public RuleLevel Level { get; }

        public string Message { get; }
    }
}
