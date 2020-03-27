﻿// -----------------------------------------------------------------------
// <copyright file="TupleObjectInfo.cs" company="Project Contributors">
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
using MicroLite.Logging;

namespace MicroLite.Mapping
{
    [System.Diagnostics.DebuggerDisplay("ObjectInfo for {ForType}")]
    internal sealed class TupleObjectInfo : IObjectInfo
    {
        private static readonly Type s_forType = typeof(Tuple);
        private static readonly ILog s_log = LogManager.GetCurrentClassLog();

        public Type ForType => s_forType;

        public TableInfo TableInfo => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

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

            var fieldTypes = new Type[reader.FieldCount];
            object[] values = new object[reader.FieldCount];

            for (int i = 0; i < reader.FieldCount; i++)
            {
                fieldTypes[i] = reader.GetFieldType(i);
                values[i] = reader.IsDBNull(i) ? null : reader.GetValue(i);
            }

            Type tupleType = GetTupleType(fieldTypes);

            object tuple = Activator.CreateInstance(tupleType, values);

            return tuple;
        }

        public ColumnInfo GetColumnInfo(string columnName)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        public object GetIdentifierValue(object instance)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        public SqlArgument[] GetInsertValues(object instance)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        public SqlArgument[] GetUpdateValues(object instance)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        public bool HasDefaultIdentifierValue(object instance)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        public bool IsDefaultIdentifier(object identifier)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        public void SetIdentifierValue(object instance, object identifier)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        public void VerifyInstanceForInsert(object instance)
            => throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_NotSupportedReason);

        private static Type GetTupleType(Type[] fieldTypes)
        {
            switch (fieldTypes.Length)
            {
                case 1:
                    return typeof(Tuple<>).MakeGenericType(fieldTypes);

                case 2:
                    return typeof(Tuple<,>).MakeGenericType(fieldTypes);

                case 3:
                    return typeof(Tuple<,,>).MakeGenericType(fieldTypes);

                case 4:
                    return typeof(Tuple<,,,>).MakeGenericType(fieldTypes);

                case 5:
                    return typeof(Tuple<,,,,>).MakeGenericType(fieldTypes);

                case 6:
                    return typeof(Tuple<,,,,,>).MakeGenericType(fieldTypes);

                case 7:
                    return typeof(Tuple<,,,,,,>).MakeGenericType(fieldTypes);

                default:
                    throw new NotSupportedException(ExceptionMessages.TupleObjectInfo_TupleNotSupported);
            }
        }
    }
}
