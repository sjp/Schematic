using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;

namespace SJP.Schematic.Serialization
{
    public class JsonRelationalDatabaseSerializer : IRelationalDatabaseSerializer
    {
        public Task<IRelationalDatabase> DeserializeAsync(string input, CancellationToken cancellationToken = default)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var dto = JsonSerializer.Deserialize<Dto.RelationalDatabase>(input, _settings.Value);
            return Task.FromResult(dto.FromDto());
        }

        public Task<string> SerializeAsync(IRelationalDatabase obj, CancellationToken cancellationToken = default)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return SerializeAsyncCore(obj, cancellationToken);
        }

        private static async Task<string> SerializeAsyncCore(IRelationalDatabase obj, CancellationToken cancellationToken)
        {
            var dto = await obj.ToDto(cancellationToken).ConfigureAwait(false);
            return JsonSerializer.Serialize(dto, _settings.Value);
        }

        private readonly static Lazy<JsonSerializerOptions> _settings = new Lazy<JsonSerializerOptions>(LoadSettings);

        private static JsonSerializerOptions LoadSettings()
        {
            var settings = new JsonSerializerOptions
            {
                WriteIndented = true,
                IgnoreNullValues = true,
            };
            settings.Converters.Add(new JsonStringEnumConverter());

            return settings;
        }
    }
}
