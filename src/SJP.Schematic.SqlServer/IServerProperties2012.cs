using System;

namespace SJP.Schematic.SqlServer
{
    public interface IServerProperties2012
    {
        Version BuildClrVersion { get; }
        string Collation { get; }
        int CollationID { get; }
        int ComparisonStyle { get; }
        string ComputerNamePhysicalNetBIOS { get; }
        string Edition { get; }
        long EditionID { get; }
        int EngineEdition { get; }
        int FilestreamConfiguredLevel { get; }
        int FilestreamEffectiveLevel { get; }
        string FilestreamShareName { get; }
        int? HadrManagerStatus { get; }
        string InstanceDefaultDataPath { get; }
        string InstanceDefaultLogPath { get; }
        string InstanceName { get; }
        bool IsAdvancedAnalyticsInstalled { get; }
        bool? IsClustered { get; }
        bool? IsFullTextInstalled { get; }
        bool? IsHadrEnabled { get; set; }
        bool? IsIntegratedSecurityOnly { get; }
        bool? IsLocalDB { get; set; }
        bool? IsSingleUser { get; }
        int LCID { get; }
        string LicenseType { get; }
        string MachineName { get; }
        int? NumLicenses { get; }
        int? ProcessID { get; }
        string ProductBuildType { get; set; }
        string ProductLevel { get; }
        int ProductMajorVersion { get; set; }
        int ProductMinorVersion { get; set; }
        string ProductUpdateLevel { get; set; }
        string ProductUpdateReference { get; set; }
        Version ProductVersion { get; }
        DateTime ResourceLastUpdateDateTime { get; }
        Version ResourceVersion { get; }
        string ServerName { get; }
        int SqlCharSet { get; }
        string SqlCharSetName { get; }
        int SqlSortOrder { get; }
        string SqlSortOrderName { get; }
    }
}