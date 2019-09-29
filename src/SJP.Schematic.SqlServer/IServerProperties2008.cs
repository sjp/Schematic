using System;

namespace SJP.Schematic.SqlServer
{
    public interface IServerProperties2008
    {
        Version? BuildClrVersion { get; }
        string? Collation { get; }
        int CollationID { get; }
        int ComparisonStyle { get; }
        string? ComputerNamePhysicalNetBIOS { get; }
        string? Edition { get; }
        long EditionID { get; }
        int EngineEdition { get; }
        int FilestreamConfiguredLevel { get; }
        int FilestreamEffectiveLevel { get; }
        string? FilestreamShareName { get; }
        string? InstanceName { get; }
        bool IsAdvancedAnalyticsInstalled { get; }
        bool? IsClustered { get; }
        bool? IsFullTextInstalled { get; }
        bool? IsIntegratedSecurityOnly { get; }
        bool? IsSingleUser { get; }
        int LCID { get; }
        string? LicenseType { get; }
        string? MachineName { get; }
        int? NumLicenses { get; }
        int? ProcessID { get; }
        string? ProductLevel { get; }
        Version? ProductVersion { get; }
        DateTime ResourceLastUpdateDateTime { get; }
        Version? ResourceVersion { get; }
        string? ServerName { get; }
        int SqlCharSet { get; }
        string? SqlCharSetName { get; }
        int SqlSortOrder { get; }
        string? SqlSortOrderName { get; }
    }
}