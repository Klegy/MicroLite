﻿// -----------------------------------------------------------------------
// <copyright file="TimeSpanTypeConverter.cs" company="Project Contributors">
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

namespace MicroLite.TypeConverters
{
    /// <summary>
    /// An ITypeConverter which can convert a TimeSpan to and from the stored database value of a 64 bit integer column.
    /// </summary>
    public sealed class TimeSpanTypeConverter : ITypeConverter
    {
        private readonly Type _timeSpanType = typeof(TimeSpan);
        private readonly Type _timeSpanTypeNullable = typeof(TimeSpan?);

        /// <summary>
        /// Initialises a new instance of the <see cref="TimeSpanTypeConverter"/> class.
        /// </summary>
        public TimeSpanTypeConverter()
        {
            TypeConverter.RegisterTypeMapping(_timeSpanType, DbType.Int64);
            TypeConverter.RegisterTypeMapping(_timeSpanTypeNullable, DbType.Int64);
        }

        /// <summary>
        /// Determines whether this type converter can convert values for the specified type.
        /// </summary>
        /// <param name="type">The type to check.</param>
        /// <returns>
        ///   <c>true</c> if this instance can convert the specified type; otherwise, <c>false</c>.
        /// </returns>
        public bool CanConvert(Type type)
            => _timeSpanType == type || _timeSpanTypeNullable == type;

        /// <summary>
        /// Converts the specified database value into an instance of the specified type.
        /// </summary>
        /// <param name="value">The database value to be converted.</param>
        /// <param name="type">The type to convert to.</param>
        /// <returns>An instance of the specified type containing the specified value.</returns>
        public object ConvertFromDbValue(object value, Type type)
        {
            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (value == null || value == DBNull.Value)
            {
                return null;
            }

            var timeSpan = new TimeSpan((long)value);

            return timeSpan;
        }

        /// <summary>
        /// Converts value at the specified index in the IDataReader into an instance of the specified type.
        /// </summary>
        /// <param name="reader">The IDataReader containing the results.</param>
        /// <param name="index">The index of the record to read from the IDataReader.</param>
        /// <param name="type">The type to convert result value to.</param>
        /// <returns>An instance of the specified type containing the specified value.</returns>
        public object ConvertFromDbValue(IDataReader reader, int index, Type type)
        {
            if (reader is null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (type is null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (reader.IsDBNull(index))
            {
                return null;
            }

            long value = reader.GetInt64(index);
            var timeSpan = new TimeSpan(value);

            return timeSpan;
        }

        /// <summary>
        /// Converts the specified value into an instance of the database value.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <param name="type">The type to convert from.</param>
        /// <returns>An instance of the corresponding database type containing the value.</returns>
        public object ConvertToDbValue(object value, Type type)
        {
            if (value is null)
            {
                return value;
            }

            var timeSpan = (TimeSpan)value;

            long timeSpanTicks = timeSpan.Ticks;

            return timeSpanTicks;
        }
    }
}
