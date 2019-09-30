using System;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class DatabaseTriggerMappingExtensions
    {
        public static Dto.DatabaseTrigger ToDto(this IDatabaseTrigger trigger)
        {
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            var triggerName = trigger.Name.ToDto();

            return new Dto.DatabaseTrigger
            {
                Name = triggerName,
                Definition = trigger.Definition,
                QueryTiming = trigger.QueryTiming,
                TriggerEvent = trigger.TriggerEvent,
                IsEnabled = trigger.IsEnabled
            };
        }

        public static IDatabaseTrigger FromDto(this Dto.DatabaseTrigger trigger)
        {
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            var triggerName = trigger.Name.FromDto();
            return new DatabaseTrigger(
                (Identifier)triggerName,
                trigger.Definition!,
                trigger.QueryTiming,
                trigger.TriggerEvent,
                trigger.IsEnabled
            );
        }
    }
}
