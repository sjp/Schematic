using System;
using System.Reflection;

namespace SJP.Schematic.Modelled.Reflection
{
    /// <summary>
    /// Defines methods used to access comments on reflected type information.
    /// </summary>
    public interface IReflectionCommentProvider
    {
        /// <summary>
        /// Retrieves comments associated with a given constructor.
        /// </summary>
        /// <param name="constructorInfo">A constructor.</param>
        /// <returns>A comment for <paramref name="constructorInfo"/> if available, otherwise <c>null</c>.</returns>
        string GetComment(ConstructorInfo constructorInfo);

        /// <summary>
        /// Retrieves comments associated with a given event.
        /// </summary>
        /// <param name="eventInfo">An event.</param>
        /// <returns>A comment for <paramref name="eventInfo"/> if available, otherwise <c>null</c>.</returns>
        string GetComment(EventInfo eventInfo);

        /// <summary>
        /// Retrieves comments associated with a given field.
        /// </summary>
        /// <param name="fieldInfo">A field.</param>
        /// <returns>A comment for <paramref name="fieldInfo"/> if available, otherwise <c>null</c>.</returns>
        string GetComment(FieldInfo fieldInfo);

        /// <summary>
        /// Retrieves comments associated with a given method.
        /// </summary>
        /// <param name="methodInfo">A method.</param>
        /// <returns>A comment for <paramref name="methodInfo"/> if available, otherwise <c>null</c>.</returns>
        string GetComment(MethodInfo methodInfo);

        /// <summary>
        /// Retrieves comments associated with a given property.
        /// </summary>
        /// <param name="propertyInfo">A property.</param>
        /// <returns>A comment for <paramref name="propertyInfo"/> if available, otherwise <c>null</c>.</returns>
        string GetComment(PropertyInfo propertyInfo);

        /// <summary>
        /// Retrieves comments associated with a given type.
        /// </summary>
        /// <param name="type">A type.</param>
        /// <returns>A comment for <paramref name="type"/> if available, otherwise <c>null</c>.</returns>
        string GetComment(Type type);
    }
}
