﻿// -----------------------------------------------------------------------
// <copyright file="ExpandoObjectInfo.cs" company="Project Contributors">
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
using System.Collections.Generic;
using System.Data;
using System.Dynamic;
using MicroLite.Logging;

namespace MicroLite.Mapping
{
    [System.Diagnostics.DebuggerDisplay("ObjectInfo for {ForType}")]
    internal sealed class ExpandoObjectInfo : IObjectInfo
    {
        private static readonly Type s_forType = typeof(ExpandoObject);
        private static readonly ILog s_log = LogManager.GetCurrentClassLog();

        public Type ForType => s_forType;

        public TableInfo TableInfo => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public object CreateInstance(IDataReader reader)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (s_log.IsDebug)
            {
                s_log.Debug(LogMessages.ObjectInfo_CreatingInstance, s_forType.Name);
            }

            var instance = new ExpandoObject();
            var dictionary = (IDictionary<string, object>)instance;

            for (int i = 0; i < reader.FieldCount; i++)
            {
                string columnName = reader.GetName(i);

                if (!"MicroLiteRowNumber".Equals(columnName, StringComparison.Ordinal))
                {
                    object value = reader.IsDBNull(i) ? null : reader.GetValue(i);

                    dictionary[columnName] = value;
                }
            }

            return instance;
        }

        public ColumnInfo GetColumnInfo(string columnName) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public object GetIdentifierValue(object instance) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public SqlArgument[] GetInsertValues(object instance) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public SqlArgument[] GetUpdateValues(object instance) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public bool HasDefaultIdentifierValue(object instance) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public bool IsDefaultIdentifier(object identifier) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public void SetIdentifierValue(object instance, object identifier) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);

        public void VerifyInstanceForInsert(object instance) => throw new NotSupportedException(ExceptionMessages.ExpandoObjectInfo_NotSupportedReason);
    }
}
