namespace SJP.Schematic.DataAccess.OrmLite
{
    internal static class ProjectGenerator
    {
        public static string ProjectDefinition { get; } = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""ServiceStack.OrmLite"" Version=""5.0.2"" />
    </ItemGroup>
</Project>";
    }
}
