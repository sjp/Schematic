using System;
using System.Linq;
using SJP.Schematic.Core;
using LanguageExt;

namespace SJP.Schematic.Serialization.Mapping
{
    internal static class ViewMappingExtensions
    {
        public static Dto.DatabaseView ToDto(this IDatabaseView view)
        {
            if (view == null)
                throw new ArgumentNullException(nameof(view));

            var name = view.Name.ToDto();
            var columns = view.Columns.Select(c => c.ToDto()).ToList();

            return new Dto.DatabaseView
            {
                Name = name,
                Definition = view.Definition,
                Columns = columns,
                IsMaterialized = view.IsMaterialized
            };
        }

        public static IDatabaseView FromDto(this Dto.DatabaseView dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            if (dto.Definition == null)
                throw new ArgumentException("The given view definition is null.", nameof(dto));

            var viewName = dto.Name.FromDto();
            var columns = dto.Columns.Select(c => c.FromDto()).ToList();

            return dto.IsMaterialized
                ? new DatabaseMaterializedView((Identifier)viewName, dto.Definition, columns)
                : new DatabaseView((Identifier)viewName, dto.Definition, columns);
        }
    }
}
