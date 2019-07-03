using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace SJP.Schematic.Core.Utilities
{
    /// <summary>
    /// Combines the hash code for multiple values into a single hash code. A placeholder using ported code from corefx until it's available more widely.
    /// </summary>
    public struct HashCodeBuilder
    {
        private static readonly uint s_seed = GenerateGlobalSeed();

        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;
        private const uint Prime5 = 374761393U;

        private uint _v1, _v2, _v3, _v4;
        private uint _queue1, _queue2, _queue3;
        private uint _length;

        private static uint GenerateGlobalSeed()
        {
            var random = new Random();
            var result = random.Next() + 0L + int.MaxValue;
            return (uint)result;
        }

        /// <summary>
        /// Diffuses the hash code returned by the specified value.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <returns>The hash code that represents the single value.</returns>
        public static int Combine<T1>(T1 value1)
        {
            // Provide a way of diffusing bits from something with a limited
            // input hash space. For example, many enums only have a few
            // possible hashes, only using the bottom few bits of the code. Some
            // collections are built on the assumption that hashes are spread
            // over a larger space, so diffusing the bits may help the
            // collection work more efficiently.

            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);

                uint hash = MixEmptyState();
                hash += 4;

                hash = QueueRound(hash, hc1);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Combines two values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <returns>The hash code that represents the two values.</returns>
        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);
                var hc2 = (uint)(value2?.GetHashCode() ?? 0);

                uint hash = MixEmptyState();
                hash += 8;

                hash = QueueRound(hash, hc1);
                hash = QueueRound(hash, hc2);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Combines three values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <param name="value3">The third value to combine into the hash code.</param>
        /// <returns>The hash code that represents the three values.</returns>
        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);
                var hc2 = (uint)(value2?.GetHashCode() ?? 0);
                var hc3 = (uint)(value3?.GetHashCode() ?? 0);

                uint hash = MixEmptyState();
                hash += 12;

                hash = QueueRound(hash, hc1);
                hash = QueueRound(hash, hc2);
                hash = QueueRound(hash, hc3);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Combines four values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
        /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <param name="value3">The third value to combine into the hash code.</param>
        /// <param name="value4">The fourth value to combine into the hash code.</param>
        /// <returns>The hash code that represents the four values.</returns>
        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);
                var hc2 = (uint)(value2?.GetHashCode() ?? 0);
                var hc3 = (uint)(value3?.GetHashCode() ?? 0);
                var hc4 = (uint)(value4?.GetHashCode() ?? 0);

                Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

                v1 = Round(v1, hc1);
                v2 = Round(v2, hc2);
                v3 = Round(v3, hc3);
                v4 = Round(v4, hc4);

                uint hash = MixState(v1, v2, v3, v4);
                hash += 16;

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Combines five values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
        /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
        /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <param name="value3">The third value to combine into the hash code.</param>
        /// <param name="value4">The fourth value to combine into the hash code.</param>
        /// <param name="value5">The fifth value to combine into the hash code.</param>
        /// <returns>The hash code that represents the five values.</returns>
        public static int Combine<T1, T2, T3, T4, T5>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5)
        {
            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);
                var hc2 = (uint)(value2?.GetHashCode() ?? 0);
                var hc3 = (uint)(value3?.GetHashCode() ?? 0);
                var hc4 = (uint)(value4?.GetHashCode() ?? 0);
                var hc5 = (uint)(value5?.GetHashCode() ?? 0);

                Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

                v1 = Round(v1, hc1);
                v2 = Round(v2, hc2);
                v3 = Round(v3, hc3);
                v4 = Round(v4, hc4);

                uint hash = MixState(v1, v2, v3, v4);
                hash += 20;

                hash = QueueRound(hash, hc5);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Combines six values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
        /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
        /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
        /// <typeparam name="T6">The type of the sixth value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <param name="value3">The third value to combine into the hash code.</param>
        /// <param name="value4">The fourth value to combine into the hash code.</param>
        /// <param name="value5">The fifth value to combine into the hash code.</param>
        /// <param name="value6">The sixth value to combine into the hash code.</param>
        /// <returns>The hash code that represents the six values.</returns>
        public static int Combine<T1, T2, T3, T4, T5, T6>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6)
        {
            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);
                var hc2 = (uint)(value2?.GetHashCode() ?? 0);
                var hc3 = (uint)(value3?.GetHashCode() ?? 0);
                var hc4 = (uint)(value4?.GetHashCode() ?? 0);
                var hc5 = (uint)(value5?.GetHashCode() ?? 0);
                var hc6 = (uint)(value6?.GetHashCode() ?? 0);

                Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

                v1 = Round(v1, hc1);
                v2 = Round(v2, hc2);
                v3 = Round(v3, hc3);
                v4 = Round(v4, hc4);

                uint hash = MixState(v1, v2, v3, v4);
                hash += 24;

                hash = QueueRound(hash, hc5);
                hash = QueueRound(hash, hc6);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Combines seven values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
        /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
        /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
        /// <typeparam name="T6">The type of the sixth value to combine into the hash code.</typeparam>
        /// <typeparam name="T7">The type of the seventh value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <param name="value3">The third value to combine into the hash code.</param>
        /// <param name="value4">The fourth value to combine into the hash code.</param>
        /// <param name="value5">The fifth value to combine into the hash code.</param>
        /// <param name="value6">The sixth value to combine into the hash code.</param>
        /// <param name="value7">The seventh value to combine into the hash code.</param>
        /// <returns>The hash code that represents the seven values.</returns>
        public static int Combine<T1, T2, T3, T4, T5, T6, T7>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7)
        {
            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);
                var hc2 = (uint)(value2?.GetHashCode() ?? 0);
                var hc3 = (uint)(value3?.GetHashCode() ?? 0);
                var hc4 = (uint)(value4?.GetHashCode() ?? 0);
                var hc5 = (uint)(value5?.GetHashCode() ?? 0);
                var hc6 = (uint)(value6?.GetHashCode() ?? 0);
                var hc7 = (uint)(value7?.GetHashCode() ?? 0);

                Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

                v1 = Round(v1, hc1);
                v2 = Round(v2, hc2);
                v3 = Round(v3, hc3);
                v4 = Round(v4, hc4);

                uint hash = MixState(v1, v2, v3, v4);
                hash += 28;

                hash = QueueRound(hash, hc5);
                hash = QueueRound(hash, hc6);
                hash = QueueRound(hash, hc7);

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        /// <summary>
        /// Combines eight values into a hash code.
        /// </summary>
        /// <typeparam name="T1">The type of the first value to combine into the hash code.</typeparam>
        /// <typeparam name="T2">The type of the second value to combine into the hash code.</typeparam>
        /// <typeparam name="T3">The type of the third value to combine into the hash code.</typeparam>
        /// <typeparam name="T4">The type of the fourth value to combine into the hash code.</typeparam>
        /// <typeparam name="T5">The type of the fifth value to combine into the hash code.</typeparam>
        /// <typeparam name="T6">The type of the sixth value to combine into the hash code.</typeparam>
        /// <typeparam name="T7">The type of the seventh value to combine into the hash code.</typeparam>
        /// <typeparam name="T8">The type of the eight value to combine into the hash code.</typeparam>
        /// <param name="value1">The first value to combine into the hash code.</param>
        /// <param name="value2">The second value to combine into the hash code.</param>
        /// <param name="value3">The third value to combine into the hash code.</param>
        /// <param name="value4">The fourth value to combine into the hash code.</param>
        /// <param name="value5">The fifth value to combine into the hash code.</param>
        /// <param name="value6">The sixth value to combine into the hash code.</param>
        /// <param name="value7">The seventh value to combine into the hash code.</param>
        /// <param name="value8">The eight value to combine into the hash code.</param>
        /// <returns>The hash code that represents the eight values.</returns>
        public static int Combine<T1, T2, T3, T4, T5, T6, T7, T8>(T1 value1, T2 value2, T3 value3, T4 value4, T5 value5, T6 value6, T7 value7, T8 value8)
        {
            unchecked
            {
                var hc1 = (uint)(value1?.GetHashCode() ?? 0);
                var hc2 = (uint)(value2?.GetHashCode() ?? 0);
                var hc3 = (uint)(value3?.GetHashCode() ?? 0);
                var hc4 = (uint)(value4?.GetHashCode() ?? 0);
                var hc5 = (uint)(value5?.GetHashCode() ?? 0);
                var hc6 = (uint)(value6?.GetHashCode() ?? 0);
                var hc7 = (uint)(value7?.GetHashCode() ?? 0);
                var hc8 = (uint)(value8?.GetHashCode() ?? 0);

                Initialize(out uint v1, out uint v2, out uint v3, out uint v4);

                v1 = Round(v1, hc1);
                v2 = Round(v2, hc2);
                v3 = Round(v3, hc3);
                v4 = Round(v4, hc4);

                v1 = Round(v1, hc5);
                v2 = Round(v2, hc6);
                v3 = Round(v3, hc7);
                v4 = Round(v4, hc8);

                uint hash = MixState(v1, v2, v3, v4);
                hash += 32;

                hash = MixFinal(hash);
                return (int)hash;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Rol(uint value, int count)
        {
            unchecked
            {
                return (value << count) | (value >> (32 - count));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Initialize(out uint v1, out uint v2, out uint v3, out uint v4)
        {
            unchecked
            {
                v1 = s_seed + Prime1 + Prime2;
                v2 = s_seed + Prime2;
                v3 = s_seed;
                v4 = s_seed - Prime1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint Round(uint hash, uint input)
        {
            unchecked
            {
                hash += input * Prime2;
                hash = Rol(hash, 13);
                hash *= Prime1;
                return hash;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint QueueRound(uint hash, uint queuedValue)
        {
            unchecked
            {
                hash += queuedValue * Prime3;
                return Rol(hash, 17) * Prime4;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixState(uint v1, uint v2, uint v3, uint v4)
        {
            unchecked
            {
                return Rol(v1, 1) + Rol(v2, 7) + Rol(v3, 12) + Rol(v4, 18);
            }
        }

        private static uint MixEmptyState()
        {
            unchecked
            {
                return s_seed + Prime5;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixFinal(uint hash)
        {
            unchecked
            {
                hash ^= hash >> 15;
                hash *= Prime2;
                hash ^= hash >> 13;
                hash *= Prime3;
                hash ^= hash >> 16;
                return hash;
            }
        }

        /// <summary>
        /// Adds a single value to the hash code.
        /// </summary>
        /// <typeparam name="T">The type of the value to add to the hash code.</typeparam>
        /// <param name="value">The value to add to the hash code.</param>
        public void Add<T>(T value)
        {
            unchecked
            {
                Add(value?.GetHashCode() ?? 0);
            }
        }

        /// <summary>
        /// Adds a single value to the hash code, specifying the type that provides the hash code function.
        /// </summary>
        /// <typeparam name="T">The type of the value to add to the hash code.</typeparam>
        /// <param name="value">The value to add to the hash code.</param>
        /// <param name="comparer">The <see cref="IEqualityComparer{T}"/> to use to calculate the hash code. This value can be a null reference (Nothing in Visual Basic), which will use the default equality comparer for <typeparamref name="T"/>.</param>
        public void Add<T>(T value, IEqualityComparer<T> comparer)
        {
            unchecked
            {
                Add(comparer != null ? comparer.GetHashCode(value) : (value?.GetHashCode() ?? 0));
            }
        }

        private void Add(int value)
        {
            unchecked
            {
                // The original xxHash works as follows:
                // 0. Initialize immediately. We can't do this in a struct (no
                //    default ctor).
                // 1. Accumulate blocks of length 16 (4 uints) into 4 accumulators.
                // 2. Accumulate remaining blocks of length 4 (1 uint) into the
                //    hash.
                // 3. Accumulate remaining blocks of length 1 into the hash.

                // There is no need for #3 as this type only accepts ints. _queue1,
                // _queue2 and _queue3 are basically a buffer so that when
                // ToHashCode is called we can execute #2 correctly.

                // We need to initialize the xxHash32 state (_v1 to _v4) lazily (see
                // #0) nd the last place that can be done if you look at the
                // original code is just before the first block of 16 bytes is mixed
                // in. The xxHash32 state is never used for streams containing fewer
                // than 16 bytes.

                // To see what's really going on here, have a look at the Combine
                // methods.

                var val = (uint)value;

                // Storing the value of _length locally shaves of quite a few bytes
                // in the resulting machine code.
                uint previousLength = _length++;
                uint position = previousLength % 4;

                // Switch can't be inlined.

                if (position == 0)
                {
                    _queue1 = val;
                }
                else if (position == 1)
                {
                    _queue2 = val;
                }
                else if (position == 2)
                {
                    _queue3 = val;
                }
                else // position == 3
                {
                    if (previousLength == 3)
                        Initialize(out _v1, out _v2, out _v3, out _v4);

                    _v1 = Round(_v1, _queue1);
                    _v2 = Round(_v2, _queue2);
                    _v3 = Round(_v3, _queue3);
                    _v4 = Round(_v4, val);
                }
            }
        }

        /// <summary>
        /// Calculates the final hash code after consecutive <see cref="Add{T}(T)"/> invocations.
        /// </summary>
        /// <returns>The calculated hash code.</returns>
        public int ToHashCode()
        {
            unchecked
            {
                // Storing the value of _length locally shaves of quite a few bytes
                // in the resulting machine code.
                uint length = _length;

                // position refers to the *next* queue position in this method, so
                // position == 1 means that _queue1 is populated; _queue2 would have
                // been populated on the next call to Add.
                uint position = length % 4;

                // If the length is less than 4, _v1 to _v4 don't contain anything
                // yet. xxHash32 treats this differently.

                uint hash = length < 4 ? MixEmptyState() : MixState(_v1, _v2, _v3, _v4);

                // _length is incremented once per Add(Int32) and is therefore 4
                // times too small (xxHash length is in bytes, not ints).

                hash += length * 4;

                // Mix what remains in the queue

                // Switch can't be inlined right now, so use as few branches as
                // possible by manually excluding impossible scenarios (position > 1
                // is always false if position is not > 0).
                if (position > 0)
                {
                    hash = QueueRound(hash, _queue1);
                    if (position > 1)
                    {
                        hash = QueueRound(hash, _queue2);
                        if (position > 2)
                            hash = QueueRound(hash, _queue3);
                    }
                }

                hash = MixFinal(hash);
                return (int)hash;
            }
        }
    }
}