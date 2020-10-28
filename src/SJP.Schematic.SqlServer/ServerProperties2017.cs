using System;
using LanguageExt;

namespace SJP.Schematic.SqlServer
{
    internal sealed class ServerProperties2017 : IServerProperties2017
    {
        public ServerProperties2017(Query.ServerProperties2017 query)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            BuildClrVersion = query.BuildClrVersion != null ? Version.Parse(query.BuildClrVersion.TrimStart('v')) : null;
            Collation = query.Collation;
            CollationID = query.CollationID;
            ComparisonStyle = query.ComparisonStyle;
            ComputerNamePhysicalNetBIOS = query.ComputerNamePhysicalNetBIOS;
            Edition = query.Edition;
            EditionID = query.EditionID;
            EngineEdition = query.EngineEdition;
            HadrManagerStatus = query.HadrManagerStatus;
            InstanceDefaultDataPath = query.InstanceDefaultDataPath;
            InstanceDefaultLogPath = query.InstanceDefaultLogPath;
            InstanceName = query.InstanceName;
            IsAdvancedAnalyticsInstalled = query.IsAdvancedAnalyticsInstalled;
            IsClustered = query.IsClustered.Match(isClustered => Convert.ToBoolean(isClustered), () => (bool?)null);
            IsFullTextInstalled = query.IsFullTextInstalled.Match(isFullTextInstalled => Convert.ToBoolean(isFullTextInstalled), () => (bool?)null);
            IsHadrEnabled = query.IsHadrEnabled.Match(isHadrEnabled => Convert.ToBoolean(isHadrEnabled), () => (bool?)null);
            IsIntegratedSecurityOnly = query.IsIntegratedSecurityOnly.Match(isIntegratedSecurityOnly => Convert.ToBoolean(isIntegratedSecurityOnly), () => (bool?)null);
            IsLocalDB = query.IsLocalDB.Match(isLocalDb => Convert.ToBoolean(isLocalDb), () => (bool?)null);
            IsPolyBaseInstalled = query.IsPolyBaseInstalled;
            IsSingleUser = query.IsSingleUser.Match(isSingleUser => Convert.ToBoolean(isSingleUser), () => (bool?)null);
            IsXTPSupported = query.IsXTPSupported.Match(isXtpSupported => Convert.ToBoolean(isXtpSupported), () => (bool?)null);
            LCID = query.LCID;
            LicenseType = query.LicenseType;
            MachineName = query.MachineName;
            NumLicenses = query.NumLicenses;
            ProcessID = query.ProcessID;
            ProductBuild = query.ProductBuild;
            ProductBuildType = query.ProductBuildType;
            ProductLevel = query.ProductLevel;
            ProductMajorVersion = query.ProductMajorVersion;
            ProductMinorVersion = query.ProductMinorVersion;
            ProductUpdateLevel = query.ProductUpdateLevel;
            ProductUpdateReference = query.ProductUpdateReference;
            ProductVersion = query.ProductVersion != null ? Version.Parse(query.ProductVersion) : null;
            ResourceLastUpdateDateTime = query.ResourceLastUpdateDateTime;
            ResourceVersion = query.ResourceVersion != null ? Version.Parse(query.ResourceVersion) : null;
            ServerName = query.ServerName;
            SqlCharSet = query.SqlCharSet;
            SqlCharSetName = query.SqlCharSetName;
            SqlSortOrder = query.SqlSortOrder;
            SqlSortOrderName = query.SqlSortOrderName;
            FilestreamShareName = query.FilestreamShareName;
            FilestreamConfiguredLevel = query.FilestreamConfiguredLevel;
            FilestreamEffectiveLevel = query.FilestreamEffectiveLevel;
        }

        /// <summary>
        /// <para>Version of the Microsoft .NET Framework common language runtime (CLR) that was used while building the instance of SQL Server.</para>
        /// <para><c>NULL</c> means input is not valid, an error, or not applicable</para>
        /// </summary>
        public Version? BuildClrVersion { get; }

        /// <summary>
        /// <para>Name of the default collation for the server.</para>
        /// <para><c>NULL</c> means input is not valid, or an error.</para>
        /// </summary>
        public string? Collation { get; }

        /// <summary>
        /// ID of the SQL Server collation.
        /// </summary>
        public int CollationID { get; }

        /// <summary>
        /// Windows comparison style of the collation.
        /// </summary>
        public int ComparisonStyle { get; }

        /// <summary>
        /// <para>NetBIOS name of the local computer on which the instance of SQL Server is currently running.</para>
        /// <para>For a clustered instance of SQL Server on a failover cluster, this value changes as the instance of SQL Server fails over to other nodes in the failover cluster.</para>
        /// <para>On a stand-alone instance of SQL Server, this value remains constant and returns the same value as the MachineName property.</para>
        /// <para>Note: If the instance of SQL Server is in a failover cluster and you want to obtain the name of the failover clustered instance, use the MachineName property.</para>
        /// <para><c>NULL</c> means input is not valid, an error, or not applicable</para>
        /// </summary>
        public string? ComputerNamePhysicalNetBIOS { get; }

        /// <summary>
        /// <para>Installed product edition of the instance of SQL Server. Use the value of this property to determine the features and the limits, such as Compute Capacity Limits by Edition of SQL Server. 64-bit versions of the Database Engine append (64-bit) to the version.</para>
        /// Returns: <list type="bullet">
        /// <item>Enterprise Edition</item>
        /// <item>Enterprise Edition: Core-based Licensing</item>
        /// <item>Enterprise Evaluation Edition</item>
        /// <item>Business Intelligence Edition</item>
        /// <item>Developer Edition</item>
        /// <item>Express Edition</item>
        /// <item>Express Edition with Advanced Services</item>
        /// <item>Standard Edition</item>
        /// <item>Web Edition</item>
        /// <item>'SQL Azure' indicates SQL Database or SQL Data Warehouse</item>
        /// </list>
        /// </summary>
        public string? Edition { get; }

        /// <summary>
        /// <para>Installed product edition of the instance of SQL Server. Use the value of this property to determine the features and the limits, such as Compute Capacity Limits by Edition of SQL Server. 64-bit versions of the Database Engine append (64-bit) to the version.</para>
        /// Returns: <list type="bullet">
        /// <item>1804890536 = Enterprise</item>
        /// <item>1872460670 = Enterprise Edition: Core-based Licensing</item>
        /// <item>610778273 = Enterprise Evaluation</item>
        /// <item>284895786 = Business Intelligence</item>
        /// <item>-2117995310 = Developer</item>
        /// <item>-1592396055 = Express</item>
        /// <item>-133711905= Express with Advanced Services</item>
        /// <item>-1534726760 = Standard</item>
        /// <item>1293598313 = Web</item>
        /// <item>1674378470 = SQL Database or SQL Data Warehouse</item>
        /// </list>
        /// </summary>
        public long EditionID { get; }

        /// <summary>
        /// <para>Database Engine edition of the instance of SQL Server installed on the server.</para>
        /// Returns: <list type="bullet">
        /// <item>1 = Personal or Desktop Engine (Not available in SQL Server 2005 (9.x) and later versions.)</item>
        /// <item>2 = Standard (This is returned for Standard, Web, and Business Intelligence.)</item>
        /// <item>3 = Enterprise (This is returned for Evaluation, Developer, and both Enterprise editions.)</item>
        /// <item>4 = Express (This is returned for Express, Express with Tools and Express with Advanced Services)</item>
        /// <item>5 = SQL Database</item>
        /// <item>6 = SQL Data Warehouse</item>
        /// <item>8 = Managed Instance</item>
        /// </list>
        /// </summary>
        public int EngineEdition { get; }

        /// <summary>
        /// <para>Indicates whether the Always On availability groups manager has started.</para>
        /// Returns: <list type="bullet">
        /// <item>0 = Not started, pending communication.</item>
        /// <item>1 = Started and running.</item>
        /// <item>2 = Not started and failed.</item>
        /// <item>NULL = Input is not valid, an error, or not applicable.</item>
        /// </list>
        /// </summary>
        public int? HadrManagerStatus { get; }

        /// <summary>
        /// Name of the default path to the instance data files.
        /// </summary>
        public string? InstanceDefaultDataPath { get; }

        /// <summary>
        /// Name of the default path to the instance log files.
        /// </summary>
        public string? InstanceDefaultLogPath { get; }

        /// <summary>
        /// <para>Name of the instance to which the user is connected.</para>
        /// <para>Returns <c>NULL</c> if the instance name is the default instance, if the input is not valid, or error.</para>
        /// </summary>
        public string? InstanceName { get; }

        /// <summary>
        /// Returns <c>true</c> if the Advanced Analytics feature was installed during setup; <c>false</c> if Advanced Analytics was not installed.
        /// </summary>
        public bool IsAdvancedAnalyticsInstalled { get; }

        /// <summary>
        /// <para>Server instance is configured in a failover cluster.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public bool? IsClustered { get; }

        /// <summary>
        /// <para>The full-text and semantic indexing components are installed on the current instance of SQL Server.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public bool? IsFullTextInstalled { get; }

        /// <summary>
        /// <para>Always On availability groups is enabled on this server instance.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        /// <remarks>
        /// <para>For availability replicas to be created and run on an instance of SQL Server, Always On availability groups must be enabled on the server instance. For more information, see Enable and Disable AlwaysOn Availability Groups (SQL Server).</para>
        /// <para>The IsHadrEnabled property pertains only to Always On availability groups. Other high availability or disaster recovery features, such as database mirroring or log shipping, are unaffected by this server property.</para>
        /// </remarks>
        public bool? IsHadrEnabled { get; }

        /// <summary>
        /// <para>Server is in integrated security mode.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public bool? IsIntegratedSecurityOnly { get; }

        /// <summary>
        /// <para>Server is an instance of SQL Server Express LocalDB.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public bool? IsLocalDB { get; }

        /// <summary>
        /// Returns whether the server instance has the PolyBase feature installed.
        /// </summary>
        public bool IsPolyBaseInstalled { get; }

        /// <summary>
        /// <para>Server is in single-user mode.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public bool? IsSingleUser { get; }

        /// <summary>
        /// <para>Server supports In-Memory OLTP.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public bool? IsXTPSupported { get; }

        /// <summary>
        /// Windows locale identifier (LCID) of the collation.
        /// </summary>
        public int LCID { get; }

        /// <summary>
        /// Unused. License information is not preserved or maintained by the SQL Server product. Always returns DISABLED.
        /// </summary>
        public string? LicenseType { get; }

        /// <summary>
        /// <para>Windows computer name on which the server instance is running.</para>
        /// <para>For a clustered instance, an instance of SQL Server running on a virtual server on Microsoft Cluster Service, it returns the name of the virtual server.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public string? MachineName { get; }

        /// <summary>
        /// Unused. License information is not preserved or maintained by the SQL Server product. Always returns NULL.
        /// </summary>
        public int? NumLicenses { get; }

        /// <summary>
        /// <para>Process ID of the SQL Server service. ProcessID is useful in identifying which Sqlservr.exe belongs to this instance.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public int? ProcessID { get; }

        /// <summary>
        /// The build number.
        /// </summary>
        public int ProductBuild { get; }

        /// <summary>
        /// <para>Type of build of the current build.</para>
        /// Returns: <list type="bullet">
        /// <item>OD = On Demand release a specific customer.</item>
        /// <item>GDR = General Distribution Release released through windows update.</item>
        /// <item>NULL = Not applicable.</item>
        /// </list>
        /// </summary>
        public string? ProductBuildType { get; }

        /// <summary>
        /// <para>Level of the version of the instance of SQL Server.</para>
        /// Returns: <list type="bullet">
        /// <item>'RTM' = Original release version</item>
        /// <item>'SPn' = Service pack version</item>
        /// <item>'CTPn' = Community Technology Preview version</item>
        /// </list>
        /// </summary>
        public string? ProductLevel { get; }

        /// <summary>
        /// The major version.
        /// </summary>
        public int ProductMajorVersion { get; }

        /// <summary>
        /// The minor version.
        /// </summary>
        public int ProductMinorVersion { get; }

        /// <summary>
        /// <para>Update level of the current build. CU indicates a cumulative update.</para>
        /// Returns: <list type="bullet">
        /// <item>CUn = Cumulative Update</item>
        /// <item>NULL = Not applicable.</item>
        /// </list>
        /// </summary>
        public string? ProductUpdateLevel { get; }

        /// <summary>
        /// KB article for that release.
        /// </summary>
        public string? ProductUpdateReference { get; }

        /// <summary>
        /// Version of the instance of SQL Server, in the form of 'major.minor.build.revision'.
        /// </summary>
        public Version? ProductVersion { get; }

        /// <summary>
        /// Returns the date and time that the Resource database was last updated.
        /// </summary>
        public DateTime ResourceLastUpdateDateTime { get; }

        /// <summary>
        /// Returns the version Resource database.
        /// </summary>
        public Version? ResourceVersion { get; }

        /// <summary>
        /// <para>Both the Windows server and instance information associated with a specified instance of SQL Server.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, or an error.</para>
        /// </summary>
        public string? ServerName { get; }

        /// <summary>
        /// The SQL character set ID from the collation ID.
        /// </summary>
        public int SqlCharSet { get; }

        /// <summary>
        /// The SQL character set name from the collation.
        /// </summary>
        public string? SqlCharSetName { get; }

        /// <summary>
        /// The SQL sort order ID from the collation.
        /// </summary>
        public int SqlSortOrder { get; }

        /// <summary>
        /// The SQL sort order name from the collation.
        /// </summary>
        public string? SqlSortOrderName { get; }

        /// <summary>
        /// <para>The name of the share used by FILESTREAM.</para>
        /// <para>Returns <c>NULL</c> if the input is not valid, an error, or not applicable.</para>
        /// </summary>
        public string? FilestreamShareName { get; }

        /// <summary>
        /// The configured level of FILESTREAM access.
        /// </summary>
        public int FilestreamConfiguredLevel { get; }

        /// <summary>
        /// The effective level of FILESTREAM access. This value can be different than the FilestreamConfiguredLevel if the level has changed and either an instance restart or a computer restart is pending.
        /// </summary>
        public int FilestreamEffectiveLevel { get; }
    }
}
