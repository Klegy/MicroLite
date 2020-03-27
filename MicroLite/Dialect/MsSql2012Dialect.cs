﻿// -----------------------------------------------------------------------
// <copyright file="MsSql2012Dialect.cs" company="Project Contributors">
// Copyright Project Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// </copyright>
// -----------------------------------------------------------------------
using System;
using System.Data;
using System.Text;
using MicroLite.Mapping;

namespace MicroLite.Dialect
{
    /// <summary>
    /// The implementation of <see cref="ISqlDialect"/> for MsSql Server 2012 or later.
    /// </summary>
    internal sealed class MsSql2012Dialect : MsSql2005Dialect
    {
        public override SqlQuery BuildSelectInsertIdSqlQuery(IObjectInfo objectInfo)
        {
            if (objectInfo is null)
            {
                throw new ArgumentNullException(nameof(objectInfo));
            }

            if (objectInfo.TableInfo.IdentifierStrategy == IdentifierStrategy.Sequence)
            {
                return new SqlQuery("SELECT @@id");
            }

            return base.BuildSelectInsertIdSqlQuery(objectInfo);
        }

        public override SqlQuery PageQuery(SqlQuery sqlQuery, PagingOptions pagingOptions)
        {
            if (sqlQuery is null)
            {
                throw new ArgumentNullException(nameof(sqlQuery));
            }

            var arguments = new SqlArgument[sqlQuery.Arguments.Count + 2];
            Array.Copy(sqlQuery.ArgumentsArray, 0, arguments, 0, sqlQuery.Arguments.Count);
            arguments[arguments.Length - 2] = new SqlArgument(pagingOptions.Offset, DbType.Int32);
            arguments[arguments.Length - 1] = new SqlArgument(pagingOptions.Count, DbType.Int32);

            var sqlString = SqlString.Parse(sqlQuery.CommandText, Clauses.OrderBy);

            string commandText = string.IsNullOrEmpty(sqlString.OrderBy)
                ? sqlQuery.CommandText + " ORDER BY CURRENT_TIMESTAMP"
                : sqlQuery.CommandText;

            StringBuilder stringBuilder = new StringBuilder(commandText)
                .Replace(Environment.NewLine, string.Empty)
                .Append(" OFFSET ")
                .Append(SqlCharacters.GetParameterName(arguments.Length - 2))
                .Append(" ROWS FETCH NEXT ")
                .Append(SqlCharacters.GetParameterName(arguments.Length - 1))
                .Append(" ROWS ONLY");

            return new SqlQuery(stringBuilder.ToString(), arguments);
        }

        protected override string BuildInsertCommandText(IObjectInfo objectInfo)
        {
            if (objectInfo is null)
            {
                throw new ArgumentNullException(nameof(objectInfo));
            }

            string commandText = base.BuildInsertCommandText(objectInfo);

            if (objectInfo.TableInfo.IdentifierStrategy == IdentifierStrategy.Sequence)
            {
                commandText = "DECLARE @@id " + GetSqlType(objectInfo.TableInfo.IdentifierColumn) + ";"
                    + "SELECT @@id = NEXT VALUE FOR " + objectInfo.TableInfo.IdentifierColumn.SequenceName + ";"
                    + commandText;

                int firstParenthesisIndex = commandText.IndexOf('(') + 1;

                commandText = commandText.Insert(
                    firstParenthesisIndex,
                    SqlCharacters.EscapeSql(objectInfo.TableInfo.IdentifierColumn.ColumnName) + ",");

                int secondParenthesisIndex = commandText.IndexOf('(', firstParenthesisIndex) + 1;

                commandText = commandText.Insert(
                    secondParenthesisIndex,
                    "@@id,");
            }

            return commandText;
        }

        private static string GetSqlType(ColumnInfo columnInfo)
        {
            switch (columnInfo.PropertyInfo.PropertyType.Name)
            {
                case "Byte":
                    return "tinyint";

                case "Int16":
                    return "smallint";

                case "Int32":
                    return "int";

                case "Int64":
                    return "bigint";

                default:
                    throw new NotSupportedException(columnInfo.PropertyInfo.PropertyType.Name);
            }
        }
    }
}
