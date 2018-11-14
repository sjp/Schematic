using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Core.Extensions
{
    public static class OptionExtensions
    {
        public static T UnwrapSome<T>(this Option<T> input)
        {
            if (input.IsNone)
                throw new ArgumentException("The given optional object does not have a value.", nameof(input));

            return input.IfNoneUnsafe(default(T));
        }

        public static IEnumerable<Task<T>> Somes<T>(this IEnumerable<Task<Option<T>>> input)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));

            return input.Where(x => x.IsSome).Select(UnwrapSome);
        }
    }
}
