using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

internal static class DbTypeCacheTests
{
    internal sealed record TypeArgs
    {
        public Identifier TypeName { get; init; } = "varchar";
        public DataType DataType { get; init; } = DataType.Unicode;
        public string Definition { get; init; } = "varchar(255)";
        public Type ClrType { get; init; } = typeof(string);
        public bool IsFixedLength { get; init; }
        public int MaxLength { get; init; } = 255;
        public Option<INumericPrecision> NumericPrecision { get; init; } = Option<INumericPrecision>.Some(new NumericPrecision(10, 2));
        public Option<Identifier> Collation { get; init; } = Option<Identifier>.Some("en_US");
        public Option<IDbType> ElementType { get; init; } = Option<IDbType>.None;
        public IReadOnlyList<string> EnumValues { get; init; } = [];
        public Option<IDbType> BaseType { get; init; } = Option<IDbType>.None;
        public bool IsUnsigned { get; init; }
        public string ClrTypeName { get; init; }
        public Option<int> FractionalSecondsPrecision { get; init; } = Option<int>.Some(3);
    }

    private static IDbType GetOrCreate(DbTypeCache cache, TypeArgs args)
    {
        return cache.GetOrCreate(
            args.TypeName,
            args.DataType,
            args.Definition,
            args.ClrType,
            args.IsFixedLength,
            args.MaxLength,
            args.NumericPrecision,
            args.Collation,
            args.ElementType,
            args.EnumValues,
            args.BaseType,
            args.IsUnsigned,
            args.ClrTypeName,
            args.FractionalSecondsPrecision
        );
    }

    private static IDbType CreateType() => new ColumnDataType("int", DataType.Integer, "int", typeof(int), false, 4, Option<INumericPrecision>.None, Option<Identifier>.None);

    [Test]
    public static void Ctor_GivenNegativeCapacity_ThrowsArgumentOutOfRangeException()
    {
        Assert.That(() => new DbTypeCache(-1), Throws.InstanceOf<ArgumentOutOfRangeException>());
    }

    [Test]
    public static void Capacity_WhenCreatedWithoutCapacity_ReturnsDefaultCapacity()
    {
        Assert.That(new DbTypeCache().Capacity, Is.EqualTo(DbTypeCache.DefaultCapacity));
    }

    [Test]
    public static void GetOrCreate_GivenNullEnumValues_ThrowsArgumentNullException()
    {
        var cache = new DbTypeCache();

        Assert.That(() => GetOrCreate(cache, new TypeArgs { EnumValues = null }), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetOrCreate_GivenNullTypeName_ThrowsArgumentNullException()
    {
        var cache = new DbTypeCache();

        Assert.That(() => GetOrCreate(cache, new TypeArgs { TypeName = null }), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetOrCreate_GivenNullClrType_ThrowsArgumentNullException()
    {
        var cache = new DbTypeCache();

        Assert.That(() => GetOrCreate(cache, new TypeArgs { ClrType = null }), Throws.ArgumentNullException);
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("    ")]
    public static void GetOrCreate_GivenNullOrWhiteSpaceDefinition_ThrowsArgumentException(string definition)
    {
        var cache = new DbTypeCache();

        Assert.That(() => GetOrCreate(cache, new TypeArgs { Definition = definition }), Throws.InstanceOf<ArgumentException>());
    }

    [Test]
    public static void GetOrCreate_GivenInvalidArguments_DoesNotHoldAnything()
    {
        var cache = new DbTypeCache();

        Assert.That(() => GetOrCreate(cache, new TypeArgs { DataType = (DataType)(-1) }), Throws.ArgumentException);
        Assert.That(cache.Count, Is.Zero);
    }

    [Test]
    public static void GetOrCreate_GivenArguments_ReturnsTypeWithGivenValues()
    {
        var cache = new DbTypeCache();
        var args = new TypeArgs { ClrTypeName = "Example.Type", IsFixedLength = true, IsUnsigned = true };

        var type = GetOrCreate(cache, args);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(type.TypeName, Is.EqualTo(args.TypeName));
            Assert.That(type.DataType, Is.EqualTo(args.DataType));
            Assert.That(type.Definition, Is.EqualTo(args.Definition));
            Assert.That(type.ClrType, Is.EqualTo(args.ClrType));
            Assert.That(type.ClrTypeName, Is.EqualTo(args.ClrTypeName));
            Assert.That(type.IsFixedLength, Is.EqualTo(args.IsFixedLength));
            Assert.That(type.MaxLength, Is.EqualTo(args.MaxLength));
            Assert.That(type.NumericPrecision, Is.EqualTo(args.NumericPrecision));
            Assert.That(type.Collation, Is.EqualTo(args.Collation));
            Assert.That(type.FractionalSecondsPrecision, Is.EqualTo(args.FractionalSecondsPrecision));
            Assert.That(type.IsUnsigned, Is.EqualTo(args.IsUnsigned));
            Assert.That(type.ElementType.IsNone, Is.True);
            Assert.That(type.BaseType.IsNone, Is.True);
            Assert.That(type.EnumValues, Is.Empty);
        }
    }

    [Test]
    public static void GetOrCreate_GivenIdenticalArguments_ReturnsSameInstance()
    {
        var cache = new DbTypeCache();

        // equal values held in distinct objects, as they are when read from separate rows
        var first = GetOrCreate(cache, new TypeArgs { TypeName = new Identifier("pg_catalog", "varchar"), Definition = new string("varchar(255)") });
        var second = GetOrCreate(cache, new TypeArgs { TypeName = new Identifier("pg_catalog", "varchar"), Definition = new string("varchar(255)") });

        using (Assert.EnterMultipleScope())
        {
            Assert.That(second, Is.SameAs(first));
            Assert.That(cache.Count, Is.EqualTo(1));
        }
    }

    [Test]
    public static void GetOrCreate_GivenNoOptionalValues_ReturnsSameInstance()
    {
        var cache = new DbTypeCache();
        var args = new TypeArgs
        {
            NumericPrecision = Option<INumericPrecision>.None,
            Collation = Option<Identifier>.None,
            FractionalSecondsPrecision = Option<int>.None,
        };

        var first = GetOrCreate(cache, args);
        var second = GetOrCreate(cache, args);

        Assert.That(second, Is.SameAs(first));
    }

    private static IEnumerable<TestCaseData> SingleValueDifferences()
    {
        yield return new TestCaseData(new TypeArgs { TypeName = "VARCHAR" }).SetArgDisplayNames("TypeName");
        yield return new TestCaseData(new TypeArgs { TypeName = new Identifier("pg_catalog", "varchar") }).SetArgDisplayNames("TypeNameSchema");
        yield return new TestCaseData(new TypeArgs { DataType = DataType.String }).SetArgDisplayNames("DataType");
        yield return new TestCaseData(new TypeArgs { Definition = "VARCHAR(255)" }).SetArgDisplayNames("Definition");
        yield return new TestCaseData(new TypeArgs { ClrType = typeof(object) }).SetArgDisplayNames("ClrType");
        yield return new TestCaseData(new TypeArgs { ClrTypeName = "Example.Type" }).SetArgDisplayNames("ClrTypeName");
        yield return new TestCaseData(new TypeArgs { IsFixedLength = true }).SetArgDisplayNames("IsFixedLength");
        yield return new TestCaseData(new TypeArgs { MaxLength = 100 }).SetArgDisplayNames("MaxLength");
        yield return new TestCaseData(new TypeArgs { NumericPrecision = Option<INumericPrecision>.Some(new NumericPrecision(10, 3)) }).SetArgDisplayNames("Scale");
        yield return new TestCaseData(new TypeArgs { NumericPrecision = Option<INumericPrecision>.Some(new NumericPrecision(12, 2)) }).SetArgDisplayNames("Precision");
        yield return new TestCaseData(new TypeArgs { NumericPrecision = Option<INumericPrecision>.None }).SetArgDisplayNames("NoNumericPrecision");
        yield return new TestCaseData(new TypeArgs { Collation = Option<Identifier>.Some("C") }).SetArgDisplayNames("Collation");
        yield return new TestCaseData(new TypeArgs { Collation = Option<Identifier>.None }).SetArgDisplayNames("NoCollation");
        yield return new TestCaseData(new TypeArgs { FractionalSecondsPrecision = Option<int>.Some(6) }).SetArgDisplayNames("FractionalSecondsPrecision");
        yield return new TestCaseData(new TypeArgs { FractionalSecondsPrecision = Option<int>.None }).SetArgDisplayNames("NoFractionalSecondsPrecision");
        yield return new TestCaseData(new TypeArgs { IsUnsigned = true }).SetArgDisplayNames("IsUnsigned");
    }

    [TestCaseSource(nameof(SingleValueDifferences))]
    public static void GetOrCreate_GivenArgumentsDifferingInOneValue_ReturnsDifferentInstances(TypeArgs different)
    {
        var cache = new DbTypeCache();

        var first = GetOrCreate(cache, new TypeArgs());
        var second = GetOrCreate(cache, different);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(GetOrCreate(cache, different), Is.SameAs(second));
        }
    }

    private static IEnumerable<TestCaseData> UnshareableArguments()
    {
        yield return new TestCaseData(new TypeArgs { EnumValues = ["a", "b"] }).SetArgDisplayNames("EnumValues");
        yield return new TestCaseData(new TypeArgs { ElementType = Option<IDbType>.Some(CreateType()) }).SetArgDisplayNames("ElementType");
        yield return new TestCaseData(new TypeArgs { BaseType = Option<IDbType>.Some(CreateType()) }).SetArgDisplayNames("BaseType");
        yield return new TestCaseData(new TypeArgs { NumericPrecision = Option<INumericPrecision>.Some(new FakeNumericPrecision()) }).SetArgDisplayNames("OtherNumericPrecision");
    }

    [TestCaseSource(nameof(UnshareableArguments))]
    public static void GetOrCreate_GivenUnshareableArguments_ReturnsNewInstanceEachTime(TypeArgs args)
    {
        var cache = new DbTypeCache();

        var first = GetOrCreate(cache, args);
        var second = GetOrCreate(cache, args);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(second.EnumValues, Is.EqualTo(args.EnumValues));
            Assert.That(second.ElementType, Is.EqualTo(args.ElementType));
            Assert.That(second.BaseType, Is.EqualTo(args.BaseType));
            Assert.That(second.NumericPrecision, Is.EqualTo(args.NumericPrecision));
            Assert.That(cache.Count, Is.Zero);
        }
    }

    [Test]
    public static void GetOrCreate_GivenZeroCapacity_ReturnsNewInstanceEachTime()
    {
        var cache = new DbTypeCache(0);

        var first = GetOrCreate(cache, new TypeArgs());
        var second = GetOrCreate(cache, new TypeArgs());

        using (Assert.EnterMultipleScope())
        {
            Assert.That(second, Is.Not.SameAs(first));
            Assert.That(cache.Count, Is.Zero);
        }
    }

    [Test]
    public static void GetOrCreate_WhenAtCapacity_StillSharesHeldTypesButDoesNotHoldNewOnes()
    {
        var cache = new DbTypeCache(1);
        var other = new TypeArgs { MaxLength = 100 };

        var held = GetOrCreate(cache, new TypeArgs());
        var firstOther = GetOrCreate(cache, other);
        var secondOther = GetOrCreate(cache, other);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(GetOrCreate(cache, new TypeArgs()), Is.SameAs(held));
            Assert.That(secondOther, Is.Not.SameAs(firstOther));
            Assert.That(secondOther.MaxLength, Is.EqualTo(100));
            Assert.That(cache.Count, Is.EqualTo(1));
        }
    }

    [Test]
    public static void GetOrCreate_WhenCalledConcurrently_ReturnsOneInstance()
    {
        var cache = new DbTypeCache();
        var results = new ConcurrentBag<IDbType>();

        Parallel.For(0, 1000, _ => results.Add(GetOrCreate(cache, new TypeArgs())));

        using (Assert.EnterMultipleScope())
        {
            Assert.That(results.Distinct(ReferenceEqualityComparer.Instance).Count(), Is.EqualTo(1));
            Assert.That(cache.Count, Is.EqualTo(1));
        }
    }

    private sealed class FakeNumericPrecision : INumericPrecision
    {
        public int Precision => 10;

        public int Scale => 2;
    }
}
