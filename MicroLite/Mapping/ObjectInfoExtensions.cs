// -----------------------------------------------------------------------
// <copyright file="ObjectInfoExtensions.cs" company="Project Contributors">
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
using System.Globalization;
using System.IO;
using MicroLite.TypeConverters;

namespace MicroLite.Mapping
{
    /// <summary>
    /// Extension methods for <see cref="IObjectInfo"/>.
    /// </summary>
    public static class ObjectInfoExtensions
    {
        /// <summary>
        /// Emits the mappings for the specified <see cref="IObjectInfo"/> to the specified <see cref="TextWriter"/>.
        /// </summary>
        /// <param name="objectInfo">The object information to emit.</param>
        /// <param name="textWriter">The text writer to write to.</param>
        public static void EmitMappings(this IObjectInfo objectInfo, TextWriter textWriter)
        {
            if (objectInfo is null)
            {
                throw new ArgumentNullException(nameof(objectInfo));
            }

            if (textWriter is null)
            {
                throw new ArgumentNullException(nameof(textWriter));
            }

            textWriter.WriteLine("MicroLite Mapping:");
            textWriter.WriteLine("------------------");
            textWriter.Write("Class '{0}' mapped to Table '{1}'", objectInfo.ForType.FullName, objectInfo.TableInfo.Name);

            if (objectInfo.TableInfo.Schema != null)
            {
                textWriter.WriteLine(" in Schema '{0}'", objectInfo.TableInfo.Schema);
            }
            else
            {
                textWriter.WriteLine();
            }

            textWriter.WriteLine();

            foreach (ColumnInfo columnInfo in objectInfo.TableInfo.Columns)
            {
                Type actualPropertyType = TypeConverter.ResolveActualType(columnInfo.PropertyInfo.PropertyType);

                textWriter.WriteLine(
                    "Property '{0} ({1})' mapped to Column '{2} (DbType.{3})'",
                    columnInfo.PropertyInfo.Name,
                    actualPropertyType == columnInfo.PropertyInfo.PropertyType ? actualPropertyType.FullName : actualPropertyType.FullName + "?",
                    columnInfo.ColumnName,
                    columnInfo.DbType.ToString());

                textWriter.WriteLine("\tAllow Insert: {0}", columnInfo.AllowInsert.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteLine("\tAllow Update: {0}", columnInfo.AllowUpdate.ToString(CultureInfo.InvariantCulture));
                textWriter.WriteLine("\tIs Identifier: {0}", columnInfo.IsIdentifier.ToString(CultureInfo.InvariantCulture));

                if (columnInfo.IsIdentifier)
                {
                    textWriter.WriteLine("\tIdentifier Strategy: {0}", objectInfo.TableInfo.IdentifierStrategy.ToString());
                }

                if (columnInfo.SequenceName != null)
                {
                    textWriter.WriteLine("\tSequence Name: {0}", columnInfo.SequenceName);
                }

                textWriter.WriteLine();
            }
        }

        /// <summary>
        /// Emits the mappings for the specified <see cref="IObjectInfo"/> to the <see cref="Console"/>.
        /// </summary>
        /// <param name="objectInfo">The object information to emit.</param>
        public static void EmitMappingsToConsole(this IObjectInfo objectInfo)
        {
            if (Console.Out != null)
            {
                EmitMappings(objectInfo, Console.Out);
            }
        }
    }
}
