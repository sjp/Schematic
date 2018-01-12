namespace SJP.Schematic.DataAccess.Poco
{
    internal static class ProjectGenerator
    {
        public static string ProjectDefinition { get; } = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
    </PropertyGroup>
</Project>";
    }
}
