﻿using System;
using System.Collections.Generic;
using NUnit.Framework;
using SJP.Schematic.Core.Utilities;

namespace SJP.Schematic.Core.Tests.Utilities;

[TestFixture]
internal static class VersionResolvingFactoryTests
{
    [Test]
    public static void Ctor_GivenNullDictionary_ThrowsArgumentNullException()
    {
        Assert.That(() => new VersionResolvingFactory<int>(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void Ctor_GivenEmptyDictionary_ThrowsArgumentException()
    {
        var dictionary = new Dictionary<Version, Func<int>>();
        Assert.That(() => new VersionResolvingFactory<int>(dictionary), Throws.ArgumentException);
    }

    [Test]
    public static void GetValue_GivenNullVersion_ThrowsArgmentNullException()
    {
        var dictionary = new Dictionary<Version, Func<int>>
        {
            [new Version(1, 0)] = () => 1,
            [new Version(1, 1)] = () => 2
        };
        var versionDictionary = new VersionResolvingFactory<int>(dictionary);

        Assert.That(() => versionDictionary.GetValue(null), Throws.ArgumentNullException);
    }

    [Test]
    public static void GetValue_GivenVersionLessThanLowestInCtor_ReturnsLowestInCtor()
    {
        var dictionary = new Dictionary<Version, Func<int>>
        {
            [new Version(1, 0)] = () => 1,
            [new Version(1, 1)] = () => 2
        };
        var versionDictionary = new VersionResolvingFactory<int>(dictionary);

        var value = versionDictionary.GetValue(new Version(0, 1));

        Assert.That(value, Is.EqualTo(1));
    }

    [Test]
    public static void GetValue_GivenVersionMoreThanHighestInCtor_ReturnsHighestInCtor()
    {
        var dictionary = new Dictionary<Version, Func<int>>
        {
            [new Version(1, 0)] = () => 1,
            [new Version(1, 1)] = () => 2
        };
        var versionDictionary = new VersionResolvingFactory<int>(dictionary);

        var value = versionDictionary.GetValue(new Version(2, 0));

        Assert.That(value, Is.EqualTo(2));
    }

    [Test]
    public static void GetValue_GivenVersionEqualToMiddle_ReturnsValueFromMiddleVersion()
    {
        var dictionary = new Dictionary<Version, Func<int>>
        {
            [new Version(1, 0)] = () => 1,
            [new Version(1, 1)] = () => 2,
            [new Version(2, 0)] = () => 3
        };
        var versionDictionary = new VersionResolvingFactory<int>(dictionary);

        var value = versionDictionary.GetValue(new Version(1, 1));

        Assert.That(value, Is.EqualTo(2));
    }

    [Test]
    public static void GetValue_GivenVersionAboveMiddle_ReturnsValueFromMiddleVersion()
    {
        var dictionary = new Dictionary<Version, Func<int>>
        {
            [new Version(1, 0)] = () => 1,
            [new Version(1, 1)] = () => 2,
            [new Version(2, 0)] = () => 3
        };
        var versionDictionary = new VersionResolvingFactory<int>(dictionary);

        var value = versionDictionary.GetValue(new Version(1, 5));

        Assert.That(value, Is.EqualTo(2));
    }
}