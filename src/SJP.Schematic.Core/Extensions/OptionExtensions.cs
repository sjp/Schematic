using System;
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
    }
}
