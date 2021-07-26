using System;
using System.IO;
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

        public async Task SerializeAsync(Stream stream, IRelationalDatabase database, CancellationToken cancellationToken = default)
        {
            var dto = await Mapper.ToDtoAsync(database, cancellationToken).ConfigureAwait(false);
            await JsonSerializer.SerializeAsync(stream, dto, _settings.Value, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IRelationalDatabase> DeserializeAsync(Stream stream, IIdentifierResolutionStrategy identifierResolver, CancellationToken cancellationToken = default)
        {
            var dto = await JsonSerializer.DeserializeAsync<Dto.RelationalDatabase>(stream, _settings.Value, cancellationToken);
            if (dto == null)
                throw new InvalidOperationException("Unable to parse the given JSON as a database definition.");

            dto.IdentifierResolver = identifierResolver;
            return Mapper.Map<Dto.RelationalDatabase, RelationalDatabase>(dto);
        }

        private static readonly Lazy<JsonSerializerOptions> _settings = new(LoadSettings);

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
