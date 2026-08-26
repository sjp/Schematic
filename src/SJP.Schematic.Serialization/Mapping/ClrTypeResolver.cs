using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SJP.Schematic.Serialization.Mapping;

/// <summary>
/// Resolves the CLR types named by serialized documents.
/// </summary>
/// <remarks>
/// Type names are written without any assembly information, so resolution searches every assembly
/// that is already loaded instead of only the core library. Assemblies are never loaded on demand,
/// i.e. a name present in a document cannot cause an assembly to be located and loaded.
/// </remarks>
internal static class ClrTypeResolver
{
    /// <summary>
    /// Resolves the type declared by a loaded assembly with the given name.
    /// </summary>
    /// <param name="typeName">A type name, e.g. <c>System.String</c>. An assembly-qualified name is also accepted.</param>
    /// <returns>The named type, or <see langword="null"/> when no loaded assembly declares it.</returns>
    /// <remarks>Where more than one loaded assembly declares the same type name, the first match wins.</remarks>
    public static Type? Resolve(string typeName)
    {
        ArgumentNullException.ThrowIfNull(typeName);

        try
        {
            return Type.GetType(typeName, ResolveLoadedAssembly, ResolveTypeInLoadedAssemblies, throwOnError: false, ignoreCase: false);
        }
        catch (FileLoadException)
        {
            // a malformed assembly-qualified name, e.g. one with a bad assembly name component
            return null;
        }
        catch (TypeLoadException)
        {
            // a well-formed name describing a type that cannot exist, e.g. a reference to a reference
            return null;
        }
    }

    private static Assembly? ResolveLoadedAssembly(AssemblyName assemblyName)
    {
        return Array.Find(
            AppDomain.CurrentDomain.GetAssemblies(),
            assembly => string.Equals(assembly.GetName().Name, assemblyName.Name, StringComparison.OrdinalIgnoreCase)
        );
    }

    private static Type? ResolveTypeInLoadedAssemblies(Assembly? assembly, string typeName, bool ignoreCase)
    {
        if (assembly != null)
            return assembly.GetType(typeName, throwOnError: false, ignoreCase);

        // the common case is a core library type, which this resolves without searching further
        return Type.GetType(typeName)
            ?? AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(loadedAssembly => loadedAssembly.GetType(typeName, throwOnError: false, ignoreCase))
                .FirstOrDefault(resolvedType => resolvedType != null);
    }
}
