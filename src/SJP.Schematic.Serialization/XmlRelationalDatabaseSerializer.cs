using System;
using System.Threading;
using System.Threading.Tasks;
using SJP.Schematic.Core;

namespace SJP.Schematic.Serialization
{
    public class XmlRelationalDatabaseSerializer : IRelationalDatabaseSerializer
    {
        public IRelationalDatabase Deserialize(string input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            throw new NotImplementedException();
        }

        public Task<IRelationalDatabase> DeserializeAsync(string input, CancellationToken cancellationToken = default)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            throw new NotImplementedException();
        }

        public string Serialize(IRelationalDatabase obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            throw new NotImplementedException();
        }

        public Task<string> SerializeAsync(IRelationalDatabase obj, CancellationToken cancellationToken = default)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            throw new NotImplementedException();
        }
    }
}
