using System;
using System.Data;
using System.Linq;
using SJP.Schematic.Core;
using SJP.Schematic.Core.Extensions;

namespace SJP.Schematic.Oracle
{
    public class OracleDatabaseIdentifierLengthValidation : IOracleDatabaseIdentifierValidation
    {
        public OracleDatabaseIdentifierLengthValidation(uint maxLength)
        {
            if (maxLength == 0)
                throw new ArgumentException("The maximum identifier length must be at least 1.", nameof(maxLength));

            MaxIdentifierLength = maxLength;
        }

        protected virtual uint MaxIdentifierLength { get; }

        public bool IsValidIdentifier(Identifier identifier)
        {
            if (identifier == null)
                throw new ArgumentNullException(nameof(identifier));

            var components = new[] { identifier.Server, identifier.Database, identifier.Schema, identifier.LocalName };
            return components.All(IsIdentifierComponentValid);
        }

        // basically, are all of the characters ascii (equivalent to a byte for oracle's purposes)
        // and are all of the ascii characters no longer than the database's max identifier length
        private bool IsIdentifierComponentValid(string? component)
        {
            // let's assume null is equivalent to a null literal and always valid
            if (component == null)
                return true;

            var asciiByteCount = GetAsciiByteCount(component);
            return asciiByteCount == component.Length
                && asciiByteCount <= MaxIdentifierLength;
        }

        private static int GetAsciiByteCount(string component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component));

            return component.Count(c => c < 128);
        }
    }
}