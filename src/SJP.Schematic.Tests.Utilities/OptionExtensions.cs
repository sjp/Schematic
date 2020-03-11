using System;
using System.Threading.Tasks;
using LanguageExt;

namespace SJP.Schematic.Tests.Utilities
{
    public static class OptionExtensions
    {
        public static T UnwrapSome<T>(this Option<T> input)
        {
            if (input.IsNone)
                throw new ArgumentException("The given optional object does not have a value.", nameof(input));

            return input.IfNoneUnsafe(default(T)!);
        }

        public static async Task<T> UnwrapSomeAsync<T>(this OptionAsync<T> input)
        {
            var isNone = await input.IsNone.ConfigureAwait(false);
            if (isNone)
                throw new ArgumentException("The given optional object does not have a value.", nameof(input));

            return await input.IfNoneUnsafe(default(T)!).ConfigureAwait(false);
        }
    }
}
