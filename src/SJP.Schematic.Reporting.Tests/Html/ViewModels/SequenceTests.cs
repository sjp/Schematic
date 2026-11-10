using System;
using LanguageExt;
using NUnit.Framework;
using SJP.Schematic.Reporting.Html.ViewModels;

namespace SJP.Schematic.Reporting.Tests.Html.ViewModels;

internal static class SequenceTests
{
    [Test]
    public static void Ctor_GivenNullSequenceName_ThrowsArgumentNullException()
    {
        Assert.That(
            () => new Sequence(null!, "bigint", Option<Uri>.None, 1, 1, Option<decimal>.None, Option<decimal>.None, "None", false, false),
            Throws.ArgumentNullException.With.Property(nameof(ArgumentException.ParamName)).EqualTo("sequenceName")
        );
    }
}
