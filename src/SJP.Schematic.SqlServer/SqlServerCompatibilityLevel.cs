namespace SJP.Schematic.SqlServer;

/// <summary>
/// A named representation of each of the known SQL Server versions.
/// </summary>
public enum SqlServerCompatibilityLevel
{
    /// <summary>
    /// An unknown/unmapped version of SQL Server.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// SQL Server 6.5
    /// </summary>
    SqlServer6_5 = 65,

    /// <summary>
    /// SQL Server 7.0
    /// </summary>
    SqlServer7_0 = 70,

    /// <summary>
    /// SQL Server 2000.
    /// </summary>
    SqlServer2000 = 80,

    /// <summary>
    /// SQL Server 2000.
    /// </summary>
    SqlServer2005 = 90,

    /// <summary>
    /// SQL Server 2008.
    /// </summary>
    SqlServer2008 = 100,

    /// <summary>
    /// SQL Server 2012.
    /// </summary>
    SqlServer2012 = 110,

    /// <summary>
    /// SQL Server 2014.
    /// </summary>
    SqlServer2014 = 120,

    /// <summary>
    /// SQL Server 2016.
    /// </summary>
    SqlServer2016 = 130,

    /// <summary>
    /// SQL Server 2017.
    /// </summary>
    SqlServer2017 = 140,

    /// <summary>
    /// SQL Server 2019.
    /// </summary>
    SqlServer2019 = 150,

    /// <summary>
    /// SQL Server 2022.
    /// </summary>
    SqlServer2022 = 160,

    /// <summary>
    /// SQL Server 2025.
    /// </summary>
    SqlServer2025 = 170,
}
