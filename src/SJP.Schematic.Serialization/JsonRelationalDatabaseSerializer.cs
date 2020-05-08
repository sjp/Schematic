using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using SJP.Schematic.Core;
using SJP.Schematic.Serialization.Mapping;

namespace SJP.Schematic.Serialization
{
    public class JsonRelationalDatabaseSerializer : IRelationalDatabaseSerializer
    {
        public JsonRelationalDatabaseSerializer(IMapper mapper)
        {
            Mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        protected IMapper Mapper { get; }

        public Task<IRelationalDatabase> DeserializeAsync(string input, CancellationToken cancellationToken = default)
            => DeserializeAsync(input, new VerbatimIdentifierResolutionStrategy(), cancellationToken);

        // TODO: update this so that it deserialises in async way
        public Task<IRelationalDatabase> DeserializeAsync(string input, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            if (identifierResolver == null)
                throw new ArgumentNullException(nameof(identifierResolver));

            var dto = JsonSerializer.Deserialize<Dto.RelationalDatabase>(input, _settings.Value);
            dto.IdentifierResolver = identifierResolver;
            var db = Mapper.Map<Dto.RelationalDatabase, RelationalDatabase>(dto);

            return Task.FromResult<IRelationalDatabase>(db);
        }

        public Task<string> SerializeAsync(IRelationalDatabase obj, CancellationToken cancellationToken = default)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            return SerializeAsyncCore(obj, cancellationToken);
        }

        private async Task<string> SerializeAsyncCore(IRelationalDatabase obj, CancellationToken cancellationToken)
        {
            var dto = await Mapper.ToDtoAsync(obj, cancellationToken).ConfigureAwait(false);
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
