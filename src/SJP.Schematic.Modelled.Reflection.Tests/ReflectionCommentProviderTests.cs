using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using NUnit.Framework;
using SJP.Schematic.Modelled.Reflection.Tests.Fakes;

namespace SJP.Schematic.Modelled.Reflection.Tests
{
    [TestFixture]
    internal static class ReflectionCommentProviderTests
    {
        private static FakeReflectionCommentProvider FakeProvider { get; } = new FakeReflectionCommentProvider();

        [Test]
        public static void Ctor_GivenNullAssembly_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionCommentProvider((Assembly)null));
        }

        [Test]
        public static void Ctor_GivenNullDocument_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new ReflectionCommentProvider((XDocument)null));
        }

        [Test]
        public static void GetComment_GivenNullType_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FakeProvider.GetIdentifier((Type)null));
        }

        [Test]
        public static void GetComment_GivenNullConstructorInfo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FakeProvider.GetIdentifier((ConstructorInfo)null));
        }

        [Test]
        public static void GetComment_GivenNullEventInfo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FakeProvider.GetIdentifier((EventInfo)null));
        }

        [Test]
        public static void GetComment_GivenNullFieldInfo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FakeProvider.GetIdentifier((FieldInfo)null));
        }

        [Test]
        public static void GetComment_GivenNullMethodInfo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FakeProvider.GetIdentifier((MethodInfo)null));
        }

        [Test]
        public static void GetComment_GivenNullPropertyInfo_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => FakeProvider.GetIdentifier((PropertyInfo)null));
        }

        [Test]
        public static void GetComment_WhenIncorrectDocumentProvided_ReturnsNull()
        {
            var doc = XDocument.Parse("<a></a>");
            var type = typeof(ReflectionCommentProvider);
            var provider = new ReflectionCommentProvider(doc);
            var comment = provider.GetComment(type);

            Assert.IsNull(comment);
        }

        [Test]
        public static void GetComment_WhenMemberContainsNoNameAttribute_ReturnsNull()
        {
            var doc = XDocument.Parse("<doc><member></member></doc>");
            var type = typeof(ReflectionCommentProvider);
            var provider = new ReflectionCommentProvider(doc);
            var comment = provider.GetComment(type);

            Assert.IsNull(comment);
        }

        [Test]
        public static void GetComment_WhenMemberFoundButNoSummaryPresent_ReturnsNull()
        {
            var type = typeof(ReflectionCommentProvider);
            var fakeProvider = new FakeReflectionCommentProvider();
            var identifier = fakeProvider.GetIdentifier(type);
            var doc = XDocument.Parse($"<doc><member name=\"{ identifier }\"></member></doc>");
            var provider = new ReflectionCommentProvider(doc);
            var comment = provider.GetComment(type);

            Assert.IsNull(comment);
        }

        [Test]
        public static void GetComment_WhenIdentifierNotPresent_ReturnsNull()
        {
            var doc = XDocument.Parse("<doc><member name=\"T:A.B.C\"><summary></summary></member></doc>");
            var type = typeof(ReflectionCommentProvider);
            var provider = new ReflectionCommentProvider(doc);
            var comment = provider.GetComment(type);

            Assert.IsNull(comment);
        }

        [Test]
        public static void GetComment_WhenMemberAndSummaryPresent_ReturnsCorrectSummaryValue()
        {
            var type = typeof(ReflectionCommentProvider);
            var fakeProvider = new FakeReflectionCommentProvider();
            var identifier = fakeProvider.GetIdentifier(type);
            const string testSummary = "this is a test comment";
            var doc = XDocument.Parse($"<doc><member name=\"{ identifier }\"><summary>{ testSummary }</summary></member></doc>");
            var provider = new ReflectionCommentProvider(doc);
            var comment = provider.GetComment(type);

            Assert.AreEqual(testSummary, comment);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenPlainType_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            const string expected = "T:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass";
            var identifier = FakeProvider.GetIdentifier(type);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenGenericType_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestGenericClass<string>);
            const string expected = "T:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestGenericClass`1";
            var identifier = FakeProvider.GetIdentifier(type);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenGenericTypeWithArray_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestGenericClass<string[,,]>);
            const string expected = "T:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestGenericClass`1";
            var identifier = FakeProvider.GetIdentifier(type);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenNestedType_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass.Nested);
            const string expected = "T:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Nested";
            var identifier = FakeProvider.GetIdentifier(type);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenDefaultCtor_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var ctor = type.GetConstructors().First(c => c.GetParameters().Length == 0);
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.#ctor";
            var identifier = FakeProvider.GetIdentifier(ctor);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenCtorWithIntArg_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var ctor = type.GetConstructors().First(c => c.GetParameters().Length > 0);
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.#ctor(System.Int32)";
            var identifier = FakeProvider.GetIdentifier(ctor);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenStaticCtor_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.#cctor";
            var identifier = FakeProvider.GetIdentifier(type.TypeInitializer);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenField_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var field = type.GetField("Q");
            const string expected = "F:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Q";
            var identifier = FakeProvider.GetIdentifier(field);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenConstField_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var field = type.GetField("PI");
            const string expected = "F:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.PI";
            var identifier = FakeProvider.GetIdentifier(field);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenParameterlessMethod_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("F");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.F";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenMethodWithIntArg_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("Farg");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Farg(System.Int32)";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenMethodWithArrayArgs_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("Gg");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Gg(System.Int16[],System.Int32[][0:,0:,0:,0:][][0:,0:])";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenMethodWithRefAndOutParameters_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("Cc");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Cc(System.String,System.Int32@,System.String@)";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenMethodNamedLikeAnOperator_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("op_Implicit", new[] { typeof(TestClass) });
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.op_Implicit(SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass)";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenMethodWithGenericParameters_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("Identity");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Identity``2(``0,``1,System.Int32,``0)";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenMethodWithGenericParameterFromGenericClass_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestGenericClass<>);
            var method = type.GetMethod("IdentityTest");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestGenericClass`1.IdentityTest(`0)";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenAdditionOperator_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("op_Addition");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.op_Addition(SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass,SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass)";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenExplicitOperator_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("op_Explicit");
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.op_Explicit(SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass)~System.Collections.Generic.List{System.String[0:,0:]}";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenImplicitOperator_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var method = type.GetMethod("op_Implicit", new[] { typeof(string) });
            const string expected = "M:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.op_Implicit(System.String)~SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass";
            var identifier = FakeProvider.GetIdentifier(method);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenDelegate_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass.D);
            const string expected = "T:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.D";
            var identifier = FakeProvider.GetIdentifier(type);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenProperty_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var property = type.GetProperty("Prop");
            const string expected = "P:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Prop";
            var identifier = FakeProvider.GetIdentifier(property);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenIndexer_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var property = type.GetProperty("Item");
            const string expected = "P:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.Item(System.String)";
            var identifier = FakeProvider.GetIdentifier(property);

            Assert.AreEqual(expected, identifier);
        }

        [Test]
        public static void GetCommentByIdentifier_GivenEvent_ReturnsCorrectIdentifier()
        {
            var type = typeof(TestClass);
            var property = type.GetEvent("EventD");
            const string expected = "E:SJP.Schematic.Modelled.Reflection.Tests.ReflectionCommentProviderTests.TestClass.EventD";
            var identifier = FakeProvider.GetIdentifier(property);

            Assert.AreEqual(expected, identifier);
        }

        // Note that the following demo class is largely taken from here:
        // https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/xmldoc/processing-the-xml-file
        private sealed class TestClass
        {
            public TestClass() { }

            static TestClass() { }

            public TestClass(int i) { i++; }

            public string Q = "1";

            public const double PI = 3.14;

            public int F() { return 1; }

            public int Farg(int x) { return x; }

            // NOTE: Uncomment the following if we want to allow unsafe compilation.
            //       For now, assume that the void name is correct.
            //public int Bb(string s, ref int y, void* z) { return 1; }

            public int Cc(string s, ref int y, out string z)
            {
                s = "a";
                y = 13;
                z = "1";
                return 1;
            }

            public T1 Identity<T1, T2>(T1 t1, T2 t2, int x, T1 t1_1)
            {
                return ReferenceEquals(t1, t2) || ReferenceEquals(t2, t1_1) || ReferenceEquals(t1_1, t1) || x > 1
                    ? t1
                    : t1_1;
            }

            public int Gg(short[] array1, int[,][][,,,][] array) { return array1[0] + array[0,0][0][0,0,0,0][0]; }

            public TestClass op_Implicit(TestClass x) => x;

            public static TestClass operator +(TestClass x, TestClass xx) { return x; }

            public int Prop { get { return 1; } set { } }

#pragma warning disable 67 // unused event, just for testing
            public event D EventD;
#pragma warning restore 67

            public int this[string s] => s.Length;

            public delegate void D(int i);

            public static explicit operator List<string[,]>(TestClass x) { return new List<string[,]>(); }

            public static implicit operator TestClass(string x) { return x; }

            public class Nested
            {
            }
        }

        private sealed class TestGenericClass<T>
        {
            public int IdentityTest(T t) => ReferenceEquals(t, t) ? 0 : 1;
        }
    }
}
