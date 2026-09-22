using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Core;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class SequenceTests
{
    private static readonly Identifier SequenceName = Identifier.CreateQualifiedIdentifier("test_schema", "test_sequence");

    [Test]
    public static void Ctor_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Sequence(null!, "bigint", Option<Uri>.None, 1, 1, Option<decimal>.None, Option<decimal>.None, SequenceCacheMode.None, Option<int>.None, false, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceName")
        );
    }

    [Test]
    public static void Ctor_GivenInvalidSequenceCacheMode_ThrowsArgumentException()
    {
        Assert.That(
            () => new Sequence(SequenceName, "bigint", Option<Uri>.None, 1, 1, Option<decimal>.None, Option<decimal>.None, (SequenceCacheMode)55, Option<int>.None, false, false),
            Throws.ArgumentException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("cacheMode")
        );
    }
}
