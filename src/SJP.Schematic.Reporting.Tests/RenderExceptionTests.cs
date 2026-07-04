using System;
using NUnit.Framework;

namespace SJP.Schematic.Reporting.Tests;

[TestFixture]
internal static class RenderExceptionTests
{
    [Test]
    public static void Ctor_GivenNoArguments_HasEmptyTarget()
    {
        var exception = new RenderException();
        Assert.That(exception.Target, Is.Empty);
    }

    [Test]
    public static void Ctor_GivenMessage_SetsMessageAndHasEmptyTarget()
    {
        const string message = "test message";
        var exception = new RenderException(message);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Message, Is.EqualTo(message));
            Assert.That(exception.Target, Is.Empty);
        }
    }

    [Test]
    public static void Ctor_GivenTargetAndInnerException_SetsTargetMessageAndInnerException()
    {
        const string target = "table 'public.actor'";
        var innerException = new InvalidOperationException("boom");

        var exception = new RenderException(target, innerException);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(exception.Target, Is.EqualTo(target));
            Assert.That(exception.Message, Is.EqualTo($"Failed to render {target}."));
            Assert.That(exception.InnerException, Is.SameAs(innerException));
        }
    }
}
