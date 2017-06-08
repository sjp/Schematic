using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SJP.Schema.Core
{
    public interface IDatabaseEntity
    {
        Identifier Name { get; }

        // TODO! I think this should be implemented...
        // Maybe push into Async?
        //IEnumerable<Identifier> Dependencies { get; }
        //
        //Task<IEnumerable<Identifier>> DependenciesAsync();
        //
        //IEnumerable<Identifier> Dependents { get; }
        //
        //Task<IEnumerable<Identifier>> DependentsAsync();
    }

    public interface IDatabaseQueryable : IDatabaseEntity { }

    public interface IRelationalDatabase : IRelationalDatabaseSync, IRelationalDatabaseAsync
    {
        IDatabaseDialect Dialect { get; }
    }

    public interface IRelationalDatabaseSync
    {
        // make sure that this takes a dialect provider so that we can do quoting per vendor && version
        string DefaultSchema { get; }
        string DatabaseName { get; }

        bool TableExists(Identifier tableName);

        IReadOnlyDictionary<Identifier, IRelationalDatabaseTable> Table { get; }

        IEnumerable<IRelationalDatabaseTable> Tables { get; }

        bool ViewExists(Identifier viewName);

        IReadOnlyDictionary<Identifier, IRelationalDatabaseView> View { get; }

        IEnumerable<IRelationalDatabaseView> Views { get; }

        bool SequenceExists(Identifier sequenceName);

        IReadOnlyDictionary<Identifier, IDatabaseSequence> Sequence { get; }

        IEnumerable<IDatabaseSequence> Sequences { get; }

        bool SynonymExists(Identifier synonymName);

        IReadOnlyDictionary<Identifier, IDatabaseSynonym> Synonym { get; }

        IEnumerable<IDatabaseSynonym> Synonyms { get; }

        bool TriggerExists(Identifier triggerName);

        IReadOnlyDictionary<Identifier, IDatabaseTrigger> Trigger { get; }

        IEnumerable<IDatabaseTrigger> Triggers { get; }
    }

    public interface IDatabaseSynonym : IDatabaseEntity
    {
        Identifier Target { get; }
    }

    //TODO: suppport cancellation in every async method?
    // could be as simple as:
    //
    // Task<IEnumerable<Task<IRelationalDatabaseTable>>> TablesAsync(CancellationToken cancel = default(CancellationToken));
    //
    // would only make sense for more complicated expression

    // async analogues of every synchronous property/method
    public interface IRelationalDatabaseAsync
    {
        // TODO: make sure that this takes a dialect provider so that we can do quoting per vendor && version

        Task<bool> TableExistsAsync(Identifier tableName);

        Task<IRelationalDatabaseTable> TableAsync(Identifier tableName);

        IObservable<IRelationalDatabaseTable> TablesAsync();

        Task<bool> ViewExistsAsync(Identifier viewName);

        Task<IRelationalDatabaseView> ViewAsync(Identifier viewName);

        IObservable<IRelationalDatabaseView> ViewsAsync();

        Task<bool> SequenceExistsAsync(Identifier sequenceName);

        Task<IDatabaseSequence> SequenceAsync(Identifier sequenceName);

        IObservable<IDatabaseSequence> SequencesAsync();

        Task<bool> SynonymExistsAsync(Identifier synonymName);

        Task<IDatabaseSynonym> SynonymAsync(Identifier synonymName);

        IObservable<IDatabaseSynonym> SynonymsAsync();

        Task<bool> TriggerExistsAsync(Identifier triggerName);

        Task<IDatabaseTrigger> TriggerAsync(Identifier triggerName);

        IObservable<IDatabaseTrigger> TriggersAsync();
    }

    public interface IRelationalDatabaseTable : IDatabaseQueryable, IRelationalDatabaseTableSync, IRelationalDatabaseTableAsync { }

    public interface IRelationalDatabaseTableSync : IDatabaseEntity
    {
        IRelationalDatabase Database { get; }

        IDatabaseKey PrimaryKey { get; }

        IReadOnlyDictionary<string, IDatabaseTableColumn> Column { get; }

        IList<IDatabaseTableColumn> Columns { get; }

        IReadOnlyDictionary<string, IDatabaseCheckConstraint> CheckConstraint { get; }

        IEnumerable<IDatabaseCheckConstraint> CheckConstraints { get; }

        IReadOnlyDictionary<string, IDatabaseTableIndex> Index { get; }

        IEnumerable<IDatabaseTableIndex> Indexes { get; }

        IReadOnlyDictionary<string, IDatabaseKey> UniqueKey { get; }

        IEnumerable<IDatabaseKey> UniqueKeys { get; }

        IReadOnlyDictionary<string, IDatabaseRelationalKey> ParentKey { get; }

        IEnumerable<IDatabaseRelationalKey> ParentKeys { get; }

        IEnumerable<IDatabaseRelationalKey> ChildKeys { get; }

        // TRIGGER ON TABLE or DATABASE OR BOTH?
        IReadOnlyDictionary<string, IDatabaseTrigger> Trigger { get; }

        IEnumerable<IDatabaseTrigger> Triggers { get; }
    }

    // async analogues of every synchronous property
    public interface IRelationalDatabaseTableAsync : IDatabaseEntity
    {
        Task<IDatabaseKey> PrimaryKeyAsync();

        Task<IReadOnlyDictionary<string, IDatabaseTableColumn>> ColumnAsync();
        Task<IList<IDatabaseTableColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<string, IDatabaseCheckConstraint>> CheckConstraintAsync();
        Task<IEnumerable<IDatabaseCheckConstraint>> CheckConstraintsAsync();

        Task<IReadOnlyDictionary<string, IDatabaseTableIndex>> IndexAsync();
        Task<IEnumerable<IDatabaseTableIndex>> IndexesAsync();

        Task<IReadOnlyDictionary<string, IDatabaseKey>> UniqueKeyAsync();
        Task<IEnumerable<IDatabaseKey>> UniqueKeysAsync();

        Task<IReadOnlyDictionary<string, IDatabaseRelationalKey>> ParentKeyAsync();
        Task<IEnumerable<IDatabaseRelationalKey>> ParentKeysAsync();

        Task<IEnumerable<IDatabaseRelationalKey>> ChildKeysAsync();

        // TRIGGER ON TABLE or DATABASE OR BOTH?
        Task<IReadOnlyDictionary<string, IDatabaseTrigger>> TriggerAsync();
        Task<IEnumerable<IDatabaseTrigger>> TriggersAsync();
    }

    public interface IDatabaseTrigger : IDatabaseEntity
    {
        IRelationalDatabaseTable Table { get; }
        string Definition { get; }

        TriggerQueryTiming QueryTiming { get; }

        TriggerEvent TriggerEvent { get; }
    }

    public enum TriggerQueryTiming
    {
        Before,
        After,
        InsteadOf
    }

    [Flags]
    public enum TriggerEvent
    {
        None = 0,
        Insert = 1,
        Update = 2,
        Delete = 4,
        Truncate = 8
    }

    public interface IDatabaseOptionalConstraint
    {
        bool IsEnabled { get; }
    }

    public interface IDatabaseCheckConstraint
    {
        Identifier Name { get; }

        IRelationalDatabaseTable Table { get; }

        // something similar to Index -- need to find a generic way to build expression-like objects
        // something like var x = new Index(a, b) for a, b, both ascending
        // Index(a, b => b.Descending) for a asc, b desc
        // Index(a, b => new Expression("LOWER({0})", b))
        ISqlExpression Expression { get; }

        IEnumerable<IDatabaseColumn> DependentColumns { get; }
    }

    public interface IDatabaseViewIndex : IDatabaseIndex<IRelationalDatabaseView>
    {
        IRelationalDatabaseView View { get; }
    }

    public interface IDatabaseTableIndex : IDatabaseIndex<IRelationalDatabaseTable>
    {
        IRelationalDatabaseTable Table { get; }
    }

    public interface IDatabaseIndex<T> where T : IDatabaseQueryable
    {
        T Parent { get; }

        Identifier Name { get; }

        IEnumerable<IDatabaseIndexColumn> Columns { get; }

        IEnumerable<IDatabaseColumn> IncludedColumns { get; }

        bool IsUnique { get; }
    }

    public interface IDatabaseKey
    {
        IRelationalDatabaseTable Table { get; }

        Identifier Name { get; }

        IEnumerable<IDatabaseColumn> Columns { get; }

        DatabaseKeyType KeyType { get; }
    }

    public enum DatabaseKeyType
    {
        Primary,
        Unique,
        Foreign
    }

    public interface IDatabaseRelationalKey
    {
        IDatabaseKey ParentKey { get; }
        IDatabaseKey ChildKey { get; }
    }

    // do we need async key interfaces?
    public interface IDatabaseKeyAsync
    {
        IRelationalDatabaseTable Table { get; }

        Identifier Name { get; }

        Task<IEnumerable<IDatabaseColumn>> ColumnsAsync();
        // maybe add is primary?
        // maybe add is unique?
        // do we even care?
    }

    public interface IDatabaseRelationalKeyAsync
    {
        IDatabaseKeyAsync ParentKey { get; }

        IDatabaseKeyAsync ChildKey { get; }
    }

    public interface IRelationalDatabaseView : IDatabaseQueryable, IRelationalDatabaseViewSync, IRelationalDatabaseViewAsync
    {
        bool IsIndexed { get; }
    }

    public interface IRelationalDatabaseViewSync : IDatabaseEntity
    {
        IRelationalDatabase Database { get; }

        IReadOnlyDictionary<string, IDatabaseViewColumn> Column { get; }

        IList<IDatabaseViewColumn> Columns { get; }

        IReadOnlyDictionary<string, IDatabaseViewIndex> Index { get; }

        IEnumerable<IDatabaseViewIndex> Indexes { get; }
    }

    public interface IRelationalDatabaseViewAsync : IDatabaseEntity
    {
        Task<IReadOnlyDictionary<string, IDatabaseViewColumn>> ColumnAsync();

        Task<IList<IDatabaseViewColumn>> ColumnsAsync();

        Task<IReadOnlyDictionary<string, IDatabaseViewIndex>> IndexAsync();

        Task<IEnumerable<IDatabaseViewIndex>> IndexesAsync();
    }

    // TODO: Push relational into this?
    public interface IDatabaseView { }

    public interface IDatabaseSequence : IDatabaseEntity
    {
        int Cache { get; }

        bool Cycle { get; }

        decimal Increment { get; }

        decimal? MaxValue { get; }

        decimal? MinValue { get; }

        decimal Start { get; }
    }

    public interface IDatabaseColumn
    {
        /// <summary>
        /// TODO: This may be null!! For example, a view where a column has no alias/name
        /// </summary>
        Identifier Name { get; }

        bool IsNullable { get; }

        bool IsCalculated { get; }

        // retrieved from default constraint
        string DefaultValue { get; }

        IDbType Type { get; }

        bool IsAutoIncrement { get; }
    }

    public enum IndexColumnOrder
    {
        Ascending,
        Descending
    }

    public interface ISqlExpression
    {
        string ToSql(IDatabaseDialect dialect);

        IEnumerable<Identifier> DependentNames { get; }

        bool IsIdentity { get; }
    }

    public interface IDatabaseIndexColumn
    {
        IndexColumnOrder Order { get; }

        IList<IDatabaseColumn> DependentColumns { get; }

        string GetExpression(IDatabaseDialect dialect);
    }

    public interface IDatabaseTableColumn : IDatabaseColumn
    {
        IRelationalDatabaseTable Table { get; }
    }

    public interface IDatabaseViewColumn : IDatabaseColumn
    {
        IRelationalDatabaseView View { get; }
    }

    public interface IDatabaseStatistic<T> : IDatabaseEntity where T : IDatabaseQueryable
    {
        T Parent { get; }

        IEnumerable<IDatabaseColumn> Columns { get; }
    }

    public interface IDatabaseTableStatistic : IDatabaseStatistic<IRelationalDatabaseTable>
    {
        IRelationalDatabaseTable Table { get; }

        new IEnumerable<IDatabaseTableColumn> Columns { get; }
    }

    public interface IDatabaseViewStatistic : IDatabaseStatistic<IRelationalDatabaseView>
    {
        IRelationalDatabaseView View { get; }

        new IEnumerable<IDatabaseViewColumn> Columns { get; }
    }

    public interface IDatabaseComputedColumn : IDatabaseColumn
    {
        ISqlExpression Expression { get; }

        IEnumerable<IDatabaseColumn> DependentColumns { get; }
    }

    public enum DataType
    {
        Unknown, // error case
        BigInteger,
        Binary,
        Boolean,
        Date,
        DateTime,
        Float,
        Integer,
        Interval,
        LargeBinary,
        Numeric,
        SmallInteger,
        String,
        Text,
        Time,
        Unicode,
        UnicodeText
    }

    public abstract class ColumnDataType : IDbType
    {
        public virtual DataType Type => throw new NotImplementedException();

        public virtual bool IsFixedLength => throw new NotImplementedException();

        public virtual int Length => throw new NotImplementedException();

        public virtual Type ClrType => throw new NotImplementedException();

        // expose via IDbStringType
        public virtual bool IsUnicode => false;

        public virtual string Collation => null;

        // expose via IDbNumericType
        public virtual int Precision => UnknownLength;

        public virtual int Scale => UnknownLength;

        protected const int UnknownLength = -1;
    }

    public interface IDbType
    {
        DataType Type { get; }

        bool IsFixedLength { get; }

        int Length { get; }

        Type ClrType { get; }
    }

    public interface IDbStringType : IDbType
    {
        bool IsUnicode { get; }

        string Collation { get; }
    }

    public interface IDbNumericType : IDbType
    {
        int Precision { get; }

        int Scale { get; }
    }
}
