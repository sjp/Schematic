using System;
using System.Reflection;
using System.Xml.Linq;

namespace SJP.Schematic.Modelled.Reflection.Tests.Fakes
{
    // used for testing the identifier generation
    internal sealed class FakeReflectionCommentProvider : ReflectionCommentProvider
    {
        public FakeReflectionCommentProvider()
            : base(XDocument.Parse("<doc></doc>"))
        {
        }

        public string GetIdentifier(ConstructorInfo constructorInfo) => GetCommentIdentifier(constructorInfo);

        public string GetIdentifier(MethodInfo methodInfo) => GetCommentIdentifier(methodInfo);

        public string GetIdentifier(PropertyInfo propertyInfo) => GetCommentIdentifier(propertyInfo);

        public string GetIdentifier(EventInfo eventInfo) => GetCommentIdentifier(eventInfo);

        public string GetIdentifier(FieldInfo fieldInfo) => GetCommentIdentifier(fieldInfo);

        public string GetIdentifier(Type type) => GetCommentIdentifier(type);
    }
}
