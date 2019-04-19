using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;

namespace SJP.Schematic.Serialization
{
    public class JsonRelationalDatabaseSerializer : IRelationalDatabaseSerializer
    {
        public IRelationalDatabase Deserialize(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            var dto = JsonConvert.DeserializeObject<Dto.RelationalDatabase>(input, _settings.Value);
            return dto.FromDto();
        }

        public Task<IRelationalDatabase> DeserializeAsync(string input, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return Task.FromResult(Deserialize(input));
        }

        public string Serialize(IRelationalDatabase obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            var dto = obj.ToDto().GetAwaiter().GetResult();

            return JsonConvert.SerializeObject(dto, _settings.Value);
        }

        public Task<string> SerializeAsync(IRelationalDatabase obj, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return SerializeAsyncCore(obj, cancellationToken);
        }

        private static async Task<string> SerializeAsyncCore(IRelationalDatabase obj, CancellationToken cancellationToken)
        {
            var dto = await obj.ToDto(cancellationToken).ConfigureAwait(false);
            return JsonConvert.SerializeObject(dto, _settings.Value);
        }

        private readonly static Lazy<JsonSerializerSettings> _settings = new Lazy<JsonSerializerSettings>(LoadSettings);

        private static JsonSerializerSettings LoadSettings()
        {
            var settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                Formatting = Formatting.Indented,
                DefaultValueHandling = DefaultValueHandling.Ignore,
            };
            settings.Converters.Add(new StringEnumConverter());

            return settings;
        }
    }
}
