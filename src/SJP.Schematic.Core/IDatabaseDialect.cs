using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace SJP.Schematic.Core
{
    public interface IDatabaseDialect
    {
        string QuoteIdentifier(string identifier);

        string QuoteName(Identifier name);

        bool IsValidColumnName(Identifier name);

        // e.g. PK_TEST_123
        bool IsValidConstraintName(Identifier name);

        // i.e. table, view, sequence, etc
        bool IsValidObjectName(Identifier name);

        IDbConnection CreateConnection(string connectionString, bool openConnection = true);

        // TODO: get a type name?
        string GetTypeName(DataType dataType);

        //string ParamString { get; set; }

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
        //string GetQuotedColumnName(string columnName);
        //
        //string GetQuotedName(string columnName);
        //
        //string SanitizeFieldNameForParamName(string fieldName);
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
    }
}
