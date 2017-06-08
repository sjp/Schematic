using System;
using System.Collections.Generic;
using System.Data;

namespace SJP.Schema.Core
{
    public abstract class RelationalDatabase
    {
        protected RelationalDatabase(IDbConnection connection)
        {
            Connection = connection ?? throw new ArgumentNullException(nameof(connection));
        }

        protected IDbConnection Connection { get; }
    }

    public interface IDatabaseDialect
    {
        string QuoteIdentifier(string identifier);

        string QuoteName(Identifier name);

        bool IsValidColumnName(Identifier name);

        // e.g. PK_TEST_123
        bool IsValidConstraintName(Identifier name);

        // i.e. table, view, sequence, etc
        bool IsValidObjectName(Identifier name);

        IDbConnection CreateConnection(string connectionString);

        // TODO: get a type name?
        string GetTypeName(DataType dataType);

        //void RegisterConverter<T>(IOrmLiteConverter converter);
        //
        //IOrmLiteExecFilter ExecFilter { get; set; }
        //
        ///// <summary>
        ///// Gets the explicit Converter registered for a specific type
        ///// </summary>
        //IOrmLiteConverter GetConverter(Type type);
        //
        ///// <summary>
        ///// Return best matching converter, falling back to Enum, Value or Ref Type Converters
        ///// </summary>
        //IOrmLiteConverter GetConverterBestMatch(Type type);
        //
        //IOrmLiteConverter GetConverterBestMatch(FieldDefinition fieldDef);
        //
        //string ParamString { get; set; }
        //
        //[Obsolete("Use GetStringConverter().UseUnicode")]
        //bool UseUnicode { get; set; }
        //
        //[Obsolete("Use GetStringConverter().StringLength")]
        //int DefaultStringLength { get; set; }
        //
        //string EscapeWildcards(string value);
        //
        //INamingStrategy NamingStrategy { get; set; }
        //
        //IStringSerializer StringSerializer { get; set; }
        //
        ///// <summary>
        ///// Quote the string so that it can be used inside an SQL-expression
        ///// Escape quotes inside the string
        ///// </summary>
        ///// <param name="paramValue"></param>
        ///// <returns></returns>
        //string GetQuotedValue(string paramValue);
        //
        //string GetQuotedValue(object value, Type fieldType);
        //
        //string GetDefaultValue(Type tableType, string fieldName);
        //
        //object GetParamValue(object value, Type fieldType);
        //
        //object ToDbValue(object value, Type type);
        //
        //object FromDbValue(object value, Type type);
        //
        //object GetValue(IDataReader reader, int columnIndex, Type type);
        //
        //int GetValues(IDataReader reader, object[] values);
        //
        //IDbConnection CreateConnection(string filePath, Dictionary<string, string> options);
        //
        //string GetQuotedTableName(ModelDefinition modelDef);
        //
        //string GetQuotedTableName(string tableName, string schema = null);
        //
        //string GetQuotedColumnName(string columnName);
        //
        //string GetQuotedName(string columnName);
        //
        //string SanitizeFieldNameForParamName(string fieldName);
        //
        //string GetColumnDefinition(FieldDefinition fieldDef);
        //
        //long GetLastInsertId(IDbCommand command);
        //
        //long InsertAndGetLastInsertId<T>(IDbCommand dbCmd);
        //
        //string ToSelectStatement(Type tableType, string sqlFilter, params object[] filterParams);
        //
        //string ToSelectStatement(ModelDefinition modelDef, string selectExpression, string bodyExpression, string orderByExpression = null, int? offset = null, int? rows = null);
        //
        //string ToInsertRowStatement(IDbCommand cmd, object objWithProperties, ICollection<string> InsertFields = null);
        //
        //void PrepareParameterizedInsertStatement<T>(IDbCommand cmd, ICollection<string> insertFields = null);
        //
        //bool PrepareParameterizedUpdateStatement<T>(IDbCommand cmd, ICollection<string> updateFields = null);
        //
        //bool PrepareParameterizedDeleteStatement<T>(IDbCommand cmd, IDictionary<string, object> delteFieldValues);
        //
        //void PrepareStoredProcedureStatement<T>(IDbCommand cmd, T obj);
        //
        //void SetParameterValues<T>(IDbCommand dbCmd, object obj);
        //
        //void SetParameter(FieldDefinition fieldDef, IDbDataParameter p);
        //
        //Dictionary<string, FieldDefinition> GetFieldDefinitionMap(ModelDefinition modelDef);
        //
        //object GetFieldValue(FieldDefinition fieldDef, object value);
        //object GetFieldValue(Type fieldType, object value);
        //
        //void PrepareUpdateRowStatement(IDbCommand dbCmd, object objWithProperties, ICollection<string> UpdateFields = null);
        //
        //void PrepareUpdateRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter);
        //
        //void PrepareUpdateRowAddStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args, string sqlFilter);
        //
        //void PrepareInsertRowStatement<T>(IDbCommand dbCmd, Dictionary<string, object> args);
        //
        //string ToDeleteStatement(Type tableType, string sqlFilter, params object[] filterParams);
        //
        //IDbCommand CreateParameterizedDeleteStatement(IDbConnection connection, object objWithProperties);
        //
        //string ToExistStatement(Type fromTableType,
        //    object objWithProperties,
        //    string sqlFilter,
        //    params object[] filterParams);
        //
        //string ToSelectFromProcedureStatement(object fromObjWithProperties,
        //    Type outputModelType,
        //    string sqlFilter,
        //    params object[] filterParams);
        //
        //string ToExecuteProcedureStatement(object objWithProperties);
        //
        //string ToCreateTableStatement(Type tableType);
        //string ToPostCreateTableStatement(ModelDefinition modelDef);
        //string ToPostDropTableStatement(ModelDefinition modelDef);
        //
        //List<string> ToCreateIndexStatements(Type tableType);
        //List<string> ToCreateSequenceStatements(Type tableType);
        //string ToCreateSequenceStatement(Type tableType, string sequenceName);
        //
        //List<string> SequenceList(Type tableType);
        //
        //bool DoesTableExist(IDbConnection db, string tableName, string schema = null);
        //bool DoesTableExist(IDbCommand dbCmd, string tableName, string schema = null);
        //bool DoesColumnExist(IDbConnection db, string columnName, string tableName, string schema = null);
        //bool DoesSequenceExist(IDbCommand dbCmd, string sequencName);
        //
        //void DropColumn(IDbConnection db, Type modelType, string columnName);
        //
        //ulong FromDbRowVersion(object value);
        //SelectItem GetRowVersionColumnName(FieldDefinition field);
        //
        //string GetColumnNames(ModelDefinition modelDef);
        //SelectItem[] GetColumnNames(ModelDefinition modelDef, bool tableQualified);
        //
        //SqlExpression<T> SqlExpression<T>();
        //
        //[Obsolete("Use InitDbParam")]
        //DbType GetColumnDbType(Type columnType);
        //
        //IDbDataParameter CreateParam();
        //
        //void InitDbParam(IDbDataParameter dbParam, Type columnType);
        //
        ////DDL
        //string GetDropForeignKeyConstraints(ModelDefinition modelDef);
        //
        //string ToAddColumnStatement(Type modelType, FieldDefinition fieldDef);
        //string ToAlterColumnStatement(Type modelType, FieldDefinition fieldDef);
        //string ToChangeColumnNameStatement(Type modelType, FieldDefinition fieldDef, string oldColumnName);
        //string ToAddForeignKeyStatement<T, TForeign>(Expression<Func<T, object>> field,
        //                                             Expression<Func<TForeign, object>> foreignField,
        //                                             OnFkOption onUpdate,
        //                                             OnFkOption onDelete,
        //                                             string foreignKeyName = null);
        //string ToCreateIndexStatement<T>(Expression<Func<T, object>> field,
        //                                 string indexName = null, bool unique = false);
        //
        ////Async
        //Task OpenAsync(IDbConnection db, CancellationToken token = default(CancellationToken));
        //Task<IDataReader> ExecuteReaderAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken));
        //Task<int> ExecuteNonQueryAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken));
        //Task<object> ExecuteScalarAsync(IDbCommand cmd, CancellationToken token = default(CancellationToken));
        //Task<bool> ReadAsync(IDataReader reader, CancellationToken token = default(CancellationToken));
        //Task<List<T>> ReaderEach<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken));
        //Task<Return> ReaderEach<Return>(IDataReader reader, Action fn, Return source, CancellationToken token = default(CancellationToken));
        //Task<T> ReaderRead<T>(IDataReader reader, Func<T> fn, CancellationToken token = default(CancellationToken));
        //
        //Task<long> InsertAndGetLastInsertIdAsync<T>(IDbCommand dbCmd, CancellationToken token);
        //
        //string GetLoadChildrenSubSelect<From>(SqlExpression<From> expr);
        //string ToRowCountStatement(string innerSql);
        //
        //string ToUpdateStatement<T>(IDbCommand dbCmd, T item, ICollection<string> updateFields = null);
        //string ToInsertStatement<T>(IDbCommand dbCmd, T item, ICollection<string> insertFields = null);
        //string MergeParamsIntoSql(string sql, IEnumerable<IDbDataParameter> dbParams);
    }

    public abstract class DatabaseDialect<TDialect> : IDatabaseDialect where TDialect : IDatabaseDialect
    {
        protected DatabaseDialect()
        {

        }

        //protected static readonly ILog Log = LogManager.GetLogger(typeof(IDialectProvider));

        public abstract IDbConnection CreateConnection(string connectionString);

        public virtual string QuoteName(Identifier name)
        {
            if (name == null)
                throw new ArgumentNullException(nameof(name));

            var pieces = new List<string>();

            if (name.Database != null)
                pieces.Add(QuoteIdentifier(name.Database));
            if (name.Schema != null)
                pieces.Add(QuoteIdentifier(name.Schema));
            if (name.LocalName != null)
                pieces.Add(QuoteIdentifier(name.LocalName));

            return pieces.Join(".");
        }

        public virtual string QuoteIdentifier(string identifier)
        {
            if (identifier.IsNullOrWhiteSpace())
                throw new ArgumentNullException(nameof(identifier));

            return $"\"{ identifier.Replace("\"", "\"\"") }\"";
        }

        public abstract bool IsValidColumnName(Identifier name);

        public abstract bool IsValidConstraintName(Identifier name);

        public abstract bool IsValidObjectName(Identifier name);

        // TODO: implement mapping for abstract types to physical types
        public abstract string GetTypeName(DataType dataType);
    }
}
