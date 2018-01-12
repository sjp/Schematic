namespace SJP.Schematic.DataAccess.EntityFrameworkCore
{
    internal static class ProjectGenerator
    {
        public static string ProjectDefinition { get; } = @"<Project Sdk=""Microsoft.NET.Sdk"">
    <PropertyGroup>
        <TargetFramework>netstandard2.0</TargetFramework>
    </PropertyGroup>

    <ItemGroup>
        <PackageReference Include=""Microsoft.EntityFrameworkCore.Relational"" Version=""2.0.1"" />
    </ItemGroup>
</Project>";
    }
}
